using CdcBridge.Configuration.Models;
using CdcBridge.Core.Models;
using MsSqlCdc;

namespace CdcBridge.Application.CdcSources;

public interface IMsSqlChangesProvider
{
    Task<IEnumerable<TrackedChange>> MapChangeRowsToTrackedChanges(
        IEnumerable<AllChangeRow> changedRows,
        TrackingInstance trackingInstance);

    Task<(bool, string dbName)> CheckIsCdcEnabledOnDb();

    Task<(bool, string? tableTrakingInstanceName)> CheckIsCdcEnabledOnTable(string tableName, string? schemaName);
    
    Task<IEnumerable<AllChangeRow>> GetChangedRows(TrackingInstance trackingInstance, CdcRequest? cdcRequest = null);

    Task<(bool isEnabled, string? message, string? dbTrackingInstanceName)> CheckCdcIsEnabled(TrackingInstance trackingInstance);
}
