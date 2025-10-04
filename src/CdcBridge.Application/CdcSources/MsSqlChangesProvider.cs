using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using CdcBridge.Application.Extensions;
using CdcBridge.Configuration.Models;
using CdcBridge.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using MsSqlCdc;

namespace CdcBridge.Application.CdcSources;

public class MsSqlChangesProvider(string connectionString)
{
    public async Task<IEnumerable<TrackedChange>> MapChangeRowsToTrackedChanges(
        IEnumerable<AllChangeRow> changedRows,
        TrackingInstance trackingInstance)
    {
        await using var connection = new SqlConnection(connectionString);
        connection.Open();
        List<TrackedChange> trackedChanges = [];

        var allChangeRows = changedRows.ToArray();
        for (int i = 0; i < allChangeRows.Count(); i++)
        {
            var changedRow = allChangeRows.ElementAt(i);

            var changeTime = await Cdc.MapLsnToTimeAsync(connection, changedRow.StartLineSequenceNumber);
            
            TrackedChange trackedChange = new()
            {
                Data = new ChangeData(), 
                TrackingInstance = trackingInstance.Name,
                CreatedAt = changeTime,
                RowLabel = changedRow.StartLineSequenceNumber.ToString(),
            };
            
            switch (changedRow.Operation)
            {
                case AllChangeOperation.Delete:
                    trackedChange.ChangeType = ChangeType.Delete;
                    trackedChange.Data.Old = changedRow.Fields.ToJsonElement();
                    break;
                
                case AllChangeOperation.Insert:
                    trackedChange.ChangeType = ChangeType.Insert;
                    trackedChange.Data.New = changedRow.Fields.ToJsonElement();
                    break;
                
                case AllChangeOperation.BeforeUpdate:
                    trackedChange.ChangeType = ChangeType.Update;
                    trackedChange.Data.Old = changedRow.Fields.ToJsonElement();
                    var nextChange  = allChangeRows.ElementAt(++i);
                    trackedChange.Data.New = nextChange.Fields.ToJsonElement();
                    break;
                
                case AllChangeOperation.AfterUpdate:
                    continue;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(changedRow.Operation), changedRow.Operation, null);
            }
            
            trackedChanges.Add(trackedChange);
        }
        
        return trackedChanges;
    }

    public async Task<(bool, string dbName)> CheckIsCdcEnabledOnDb()
    {
        using IDbConnection dbConnection = new SqlConnection(connectionString);
        string dbName = dbConnection.Database;
        bool isCdcEnabled = 
            await dbConnection.QuerySingleAsync<bool>("SELECT is_cdc_enabled FROM sys.databases where name = @Name", new { Name = dbName });
        return (isCdcEnabled, dbName);
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private class CdcInstanceDbInfo
    {
        public string source_schema { get; init; }
        public string source_table { get; init; }
        public string capture_instance { get; init; }
    }

    private async Task<bool> GetTableIsTrackedByCdcColumn(string tableName, string schemaName, IDbConnection dbConnection)
    {
        string query = $"""
                       DECLARE @SchemaID INT = SCHEMA_ID('{schemaName}')
                       SELECT TOP 1 is_tracked_by_cdc FROM sys.tables
                       WHERE schema_id = @SchemaID AND name = @TableName
                       """;
        return await dbConnection.ExecuteScalarAsync<bool>(query, new { TableName = tableName });
    }
    
    public async Task<(bool, string? tableTrakingInstanceName)> CheckIsCdcEnabledOnTable(string tableName, string? schemaName)
    {
        using IDbConnection dbConnection = new SqlConnection(connectionString);
        schemaName ??= "dbo";
        
        bool isTrackedByCdcInSysTables = await GetTableIsTrackedByCdcColumn(tableName, schemaName, dbConnection);
        
        if (!isTrackedByCdcInSysTables)
            return (false, null);
        
        var isCdcEnabledOnTable = await dbConnection.QuerySingleAsync<CdcInstanceDbInfo>("""
            EXEC sys.sp_cdc_help_change_data_capture
             @source_schema = @schemaName, @source_name = @tableName
            """, new { schemaName, tableName });

        return (true, isCdcEnabledOnTable?.capture_instance);
    }
    
    public async Task<(bool isEnabled, string? message, string? dbTrackingInstanceName)> CheckCdcIsEnabled(TrackingInstance trackingInstance)
    { 
        (bool isDbEnabled, string databaseName) = await CheckIsCdcEnabledOnDb();
        
        if (!isDbEnabled)
            return (false, $"Cdc on database {databaseName} is disabled", null);
        
        (bool isTableEnabled, string? trackingInstanceName) = await CheckIsCdcEnabledOnTable(trackingInstance.SourceTable, trackingInstance.SourceSchema);

        return (isTableEnabled, !isTableEnabled ?  $"CDC on table {trackingInstance.SourceTable} is disabled" : null, trackingInstanceName);

    }


    private static ConcurrentDictionary<string,
        (bool isEnabled, string? message, string? dbTrackingInstanceName)> CdcCheckCache = new();
        
    public async Task<IEnumerable<AllChangeRow>> GetChangedRows(TrackingInstance trackingInstance,
        CdcRequest? cdcRequest = null)
    {
        
        var (isCdcEnabledOnTable, message, instanceName) = 
            CdcCheckCache.GetOrAdd(trackingInstance.Name, _ => CheckCdcIsEnabled(trackingInstance).Result);
        
        if (!isCdcEnabledOnTable || string.IsNullOrEmpty(instanceName))
            throw new DataException($"Problems getting tracked changes for instance {trackingInstance.Name} ({message})");

        await using var connection = new SqlConnection(connectionString);
        
        connection.Open();
        
        var (minLsn, maxLsn) = await ResolveBounds(instanceName, connection, cdcRequest);
        
        // В случае когда изменения еще не успели зафиксироваться
        if (minLsn >= maxLsn)
            return [];
        
        var changedRows =
            (await Cdc.GetAllChangesAsync(connection, instanceName, minLsn, maxLsn, AllChangesRowFilterOption.AllUpdateOld))
            .OrderBy(x => x.SequenceValue);
        
        return changedRows;
    }

    private async Task<(BigInteger minLsn, BigInteger maxLsn)> ResolveBounds(string trackingInstanceName, SqlConnection connection,
        CdcRequest? cdcRequest)
    {
        BigInteger minLsn = 0, maxLsn = await Cdc.GetMaxLsnAsync(connection);;
        
        if (cdcRequest == null)
        {
            minLsn = await Cdc.GetMinLsnAsync(connection, trackingInstanceName);
        }

        else if (cdcRequest.LastRowFlag != null)
        {
            minLsn = await Cdc.GetNextLsnAsync(connection,BigInteger.Parse(cdcRequest.LastRowFlag));
        }
        
        else if (cdcRequest.LastReadRowDate != null)
        {
            minLsn = await Cdc.MapTimeToLsnAsync(connection, cdcRequest.LastReadRowDate.Value, RelationalOperator.LargestLessThan);
        }
        
        return (minLsn, maxLsn);
    }
    
}

