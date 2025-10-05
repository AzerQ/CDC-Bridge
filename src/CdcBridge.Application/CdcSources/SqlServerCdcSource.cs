using System.Collections.Concurrent;
using System.Data;
using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CdcBridge.Application.CdcSources;

/// <summary>
/// Захват данных по таблицам из SQL Server
/// </summary>
public class SqlServerCdcSource : ICdcSource
{
    private static ConcurrentDictionary<Connection, MsSqlChangesProvider> _providersCache = new();
    private static MsSqlChangesProvider GetMsSqlChangesProvider(Connection connection) =>
        _providersCache.GetOrAdd(connection, con => new MsSqlChangesProvider(con.ConnectionString));
    
    public async Task<IEnumerable<TrackedChange>> GetChanges(TrackingInstanceInfo trackingInstanceInfo, CdcRequest? cdcRequest = null)
    {
       var (trackingInstance, connection) = trackingInstanceInfo;
       var msSqlChangesProvider = GetMsSqlChangesProvider(connection);
       var changedRows = await msSqlChangesProvider.GetChangedRows(trackingInstance, cdcRequest);
       return await msSqlChangesProvider.MapChangeRowsToTrackedChanges(changedRows, trackingInstance);
    }

    public async Task<(bool isEnabled, string? message, string? dbTrackingInstanceName)> CheckCdcIsEnabled(TrackingInstanceInfo trackingInstanceInfo)
    { 
        var (trackingInstance, connection) = trackingInstanceInfo;
        var msSqlChangesProvider = GetMsSqlChangesProvider(connection);
        return await msSqlChangesProvider.CheckCdcIsEnabled(trackingInstance);
    }

    public async Task EnableTrackingInstance(TrackingInstanceInfo trackingInstanceInfo)
    {
        var (trackingInstance, connection) = trackingInstanceInfo;
        var msSqlChangesProvider = GetMsSqlChangesProvider(connection);
        (bool isCdcEnabledOnDb, _) = await msSqlChangesProvider.CheckIsCdcEnabledOnDb();
        using IDbConnection dbConnection = new SqlConnection(connection.ConnectionString);
        
        if (!isCdcEnabledOnDb)
        {
            await dbConnection.ExecuteAsync("EXEC sys.sp_cdc_enable_db");
        }
        
        (bool isCdcEnabledOnTable, _) = await msSqlChangesProvider.CheckIsCdcEnabledOnTable(trackingInstance.SourceTable, trackingInstance.SourceSchema);
        
        if (isCdcEnabledOnTable)
            return;
        
        bool hasColumnsSpecification = trackingInstance.CapturedColumns.Count > 0;

        string query = """
                       EXECUTE sys.sp_cdc_enable_table
                           @source_schema = @SourceSchema,
                           @source_name = @SourceTable,
                           @role_name = NULL,
                           @supports_net_changes = 1
                       """;

        query = hasColumnsSpecification
            ? $"{query},@captured_column_list = {string.Join(",", trackingInstance.CapturedColumns)}"
            : query;
        
        await dbConnection.ExecuteAsync(query, new {trackingInstance.SourceTable, trackingInstance.SourceSchema});
        
    }

    public async Task DisableTrackingInstance(TrackingInstanceInfo trackingInstanceInfo)
    {
        var (trackingInstance, connection) = trackingInstanceInfo;
        (bool isEnabled, _, string? trackingInstanceName) = await CheckCdcIsEnabled(trackingInstanceInfo);
        
        if (isEnabled)
        {
            using IDbConnection dbConnection = new SqlConnection(connection.ConnectionString);
            await dbConnection.ExecuteAsync("""
                                            EXEC sys.sp_cdc_disable_table
                                            @source_schema = @SourceSchema,
                                            @source_name = @SourceTable,
                                            @capture_instance = @CaptureInstanceName
                                            """, 
                new {trackingInstance.SourceSchema, trackingInstance.SourceTable, CaptureInstanceName = trackingInstanceName});
        }

    }
}