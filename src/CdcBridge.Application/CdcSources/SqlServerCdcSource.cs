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
public class SqlServerCdcSource (Connection connection) : ICdcSource
{
    
    private readonly MsSqlChangesProvider _msSqlChangesProvider = new (connection.ConnectionString);
    
    public async Task<IEnumerable<TrackedChange>> GetChanges(TrackingInstance trackingInstance, CdcRequest? cdcRequest = null)
    {
        var changedRows = await _msSqlChangesProvider.GetChangedRows(trackingInstance, cdcRequest);
        return await _msSqlChangesProvider.MapChangeRowsToTrackedChanges(changedRows, trackingInstance);
    }

    public async Task<(bool isEnabled, string? message, string? dbTrackingInstanceName)> CheckCdcIsEnabled(TrackingInstance trackingInstance)
    {
       return await _msSqlChangesProvider.CheckCdcIsEnabled(trackingInstance);
    }

    public async Task EnableTrackingInstance(TrackingInstance trackingInstance)
    {
        (bool isCdcEnabledOnDb, _) = await _msSqlChangesProvider.CheckIsCdcEnabledOnDb();
        using IDbConnection dbConnection = new SqlConnection(connection.ConnectionString);
        
        if (!isCdcEnabledOnDb)
        {
            await dbConnection.ExecuteAsync("EXEC sys.sp_cdc_enable_db");
        }
        
        (bool isCdcEnabledOnTable, _) = await _msSqlChangesProvider.CheckIsCdcEnabledOnTable(trackingInstance.SourceTable, trackingInstance.SourceSchema);
        
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

    public async Task DisableTrackingInstance(TrackingInstance trackingInstance)
    {
        (bool isEnabled, _, string? trackingInstanceName) = await CheckCdcIsEnabled(trackingInstance);
        
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