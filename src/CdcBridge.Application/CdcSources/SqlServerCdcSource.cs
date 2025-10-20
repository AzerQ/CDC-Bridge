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
    private static readonly ConcurrentDictionary<Connection, IMsSqlChangesProvider> ProvidersCache = new();
    
    public static Func<Connection, IMsSqlChangesProvider> ProviderFactory { get; set; } =
        con => new MsSqlChangesProvider(con.ConnectionString);

    private static IMsSqlChangesProvider GetMsSqlChangesProvider(Connection connection) =>
        ProvidersCache.GetOrAdd(connection, ProviderFactory);

    public string Name => "SqlServer";
    
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
        
        // Используем DynamicParameters для безопасного и корректного добавления параметров
        var parameters = new DynamicParameters();
        parameters.Add("@source_schema", trackingInstance.SourceSchema ?? "dbo");
        parameters.Add("@source_name", trackingInstance.SourceTable);
        parameters.Add("@role_name", null); // Явно указываем NULL
        parameters.Add("@supports_net_changes", 1);
        
        // Базовый вызов хранимой процедуры
        var query = """
                    EXECUTE sys.sp_cdc_enable_table
                        @source_schema = @source_schema,
                        @source_name = @source_name,
                        @role_name = @role_name,
                        @supports_net_changes = @supports_net_changes
                    """;

        // Если указаны колонки, добавляем соответствующий параметр
        bool hasColumnsSpecification = trackingInstance.CapturedColumns.Any();
        if (hasColumnsSpecification)
        {
            var columnList = string.Join(",", trackingInstance.CapturedColumns);
            parameters.Add("@captured_column_list", columnList);
            // Добавляем вызов параметра в строку запроса
            query += ", @captured_column_list = @captured_column_list";
        }

        await dbConnection.ExecuteAsync(query, parameters);
        
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