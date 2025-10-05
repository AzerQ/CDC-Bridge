using LiteDB;
using LiteDB.Async;
using CdcBridge.Configuration;
using CdcBridge.Core.Models;
using CdcBridge.Persistence.Abstractions;
using CdcBridge.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace CdcBridge.Persistence;

public class LiteDbAsyncStorage : ICdcBridgeStorage, IDisposable
{
    // Меняем ILiteDatabase на ILiteDatabaseAsync
    private readonly Lazy<ILiteDatabaseAsync> _database;
    private readonly ICdcConfigurationContext _configContext;
    private readonly ILogger<LiteDbAsyncStorage> _logger;
    private const string StateCollectionName = "_state";

    public LiteDbAsyncStorage(string databasePath, ICdcConfigurationContext configContext, ILogger<LiteDbAsyncStorage> logger)
    {
        _configContext = configContext;
        _logger = logger;
        // Используем конструктор LiteDatabaseAsync
        _database = new Lazy<ILiteDatabaseAsync>(() => new LiteDatabaseAsync(databasePath));
    }

    // Методы-хелперы теперь возвращают ILiteCollectionAsync<T>
    private ILiteCollectionAsync<BufferedChange> GetChangesCollection(string trackingInstanceName) =>
        _database.Value.GetCollection<BufferedChange>(trackingInstanceName);

    private ILiteCollectionAsync<TrackingInstanceState> GetStateCollection() =>
        _database.Value.GetCollection<TrackingInstanceState>(StateCollectionName);

    public async Task<string?> GetLastProcessedRowLabelAsync(string trackingInstanceName)
    {
        // Используем FindByIdAsync
        var state = await GetStateCollection().FindByIdAsync(trackingInstanceName);
        return state?.LastProcessedRowLabel;
    }

    public async Task SaveLastProcessedRowLabelAsync(string trackingInstanceName, string rowLabel)
    {
        var state = new TrackingInstanceState
        {
            TrackingInstanceName = trackingInstanceName,
            LastProcessedRowLabel = rowLabel,
            UpdatedAtUtc = DateTime.UtcNow
        };
        // Используем UpsertAsync
        await GetStateCollection().UpsertAsync(state);
    }

    public async Task AddChangesToBufferAsync(IEnumerable<TrackedChange> changes)
    {
        foreach (var group in changes.GroupBy(c => c.TrackingInstance))
        {
            var collection = GetChangesCollection(group.Key);
            var receiversForInstance = _configContext.GetReceiversForTrackingInstance(group.Key)
                                                     .Select(r => r.Name)
                                                     .ToList();
            
            var bufferedChanges = group.Select(change =>
            {
                var bufferedChange = new BufferedChange { Change = change };
                foreach (var receiverName in receiversForInstance)
                {
                    bufferedChange.DeliveryStatuses[receiverName] = new ReceiverStatus();
                }
                return bufferedChange;
            });
            
            // Используем InsertBulkAsync
            await collection.InsertBulkAsync(bufferedChanges);
        }
    }
    
    public async Task<IEnumerable<BufferedChange>> GetPendingChangesAsync(string receiverName, string trackingInstanceName, int batchSize)
    {
        var collection = GetChangesCollection(trackingInstanceName);
        var queryExpression = $"$.DeliveryStatuses.\"{receiverName}\".Status";

        // Query() остается синхронным, но терминальные операции (ToList, First, etc.) становятся асинхронными
        var pendingChanges = await collection.Query()
            .Where(x => x.DeliveryStatuses.ContainsKey(receiverName))
            .Where(queryExpression, (int)DeliveryStatus.Pending)
            .OrderBy(x => x.BufferedAtUtc)
            .Limit(batchSize)
            .ToListAsync();
            
        return pendingChanges;
    }

    public async Task UpdateChangeStatusAsync(ObjectId changeId, string trackingInstanceName, string receiverName, bool success, string? errorMessage)
    {
        var collection = GetChangesCollection(trackingInstanceName);
        // Используем FindByIdAsync
        var change = await collection.FindByIdAsync(changeId);

        if (change != null && change.DeliveryStatuses.TryGetValue(receiverName, out var status))
        {
            status.Status = success ? DeliveryStatus.Success : DeliveryStatus.Failed;
            status.AttemptCount++;
            status.LastAttemptAtUtc = DateTime.UtcNow;
            status.ErrorDescription = errorMessage;
            // Используем UpdateAsync
            await collection.UpdateAsync(change);
        }
    }

    public async Task<int> CleanupAsync(string trackingInstanceName, TimeSpan timeToLive)
    {
        var collection = GetChangesCollection(trackingInstanceName);
        var cutoffDate = DateTime.UtcNow - timeToLive;
        _logger.LogInformation("Running cleanup for '{Instance}' on events older than {CutoffDate}", trackingInstanceName, cutoffDate);

        // Используем ToListAsync
        var candidates = await collection.Query()
            .Where(x => x.BufferedAtUtc < cutoffDate)
            .ToListAsync();
            
        var idsToDelete = candidates
            .Where(c => c.DeliveryStatuses.Values.All(s => s.Status == DeliveryStatus.Success))
            .Select(c => c.Id)
            .ToArray();
            
        if (idsToDelete.Length > 0)
        {
            // Используем DeleteManyAsync
            var deletedCount = await collection.DeleteManyAsync(x => idsToDelete.Contains(x.Id));
            _logger.LogInformation("Cleanup for '{Instance}' removed {Count} events.", trackingInstanceName, deletedCount);
            return deletedCount;
        }
        
        _logger.LogInformation("Cleanup for '{Instance}' found no events to remove.", trackingInstanceName);
        return 0;
    }

    public void Dispose()
    {
        if (_database.IsValueCreated)
        {
            _database.Value.Dispose();
        }
    }
}