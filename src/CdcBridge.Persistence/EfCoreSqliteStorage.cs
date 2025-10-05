using Microsoft.EntityFrameworkCore;
using CdcBridge.Configuration;
using CdcBridge.Core.Models;
using CdcBridge.Persistence.Abstractions;
using CdcBridge.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace CdcBridge.Persistence;

public class EfCoreSqliteStorage : ICdcBridgeStorage
{
    private readonly IDbContextFactory<CdcBridgeDbContext> _dbContextFactory;
    private readonly ICdcConfigurationContext _configContext;
    private readonly ILogger<EfCoreSqliteStorage> _logger;

    public EfCoreSqliteStorage(
        IDbContextFactory<CdcBridgeDbContext> dbContextFactory,
        ICdcConfigurationContext configContext,
        ILogger<EfCoreSqliteStorage> logger)
    {
        _dbContextFactory = dbContextFactory;
        _configContext = configContext;
        _logger = logger;
    }

    public async Task<string?> GetLastProcessedRowLabelAsync(string trackingInstanceName)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var state = await context.TrackingInstanceStates.FindAsync(trackingInstanceName);
        return state?.LastProcessedRowLabel;
    }

    public async Task SaveLastProcessedRowLabelAsync(string trackingInstanceName, string rowLabel)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var state = await context.TrackingInstanceStates.FindAsync(trackingInstanceName);
        if (state == null)
        {
            state = new TrackingInstanceState { TrackingInstanceName = trackingInstanceName };
            context.TrackingInstanceStates.Add(state);
        }
        state.LastProcessedRowLabel = rowLabel;
        state.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task AddChangesToBufferAsync(IEnumerable<TrackedChange> changes)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var newEvents = new List<BufferedChangeEvent>();

        foreach (var change in changes)
        {
            var bufferedEvent = new BufferedChangeEvent
            {
                TrackingInstanceName = change.TrackingInstance,
                RowLabel = change.RowLabel,
                Change = change, // EF Core сам сериализует это в JSON
                BufferedAtUtc = DateTime.UtcNow
            };

            var receivers = _configContext.GetReceiversForTrackingInstance(change.TrackingInstance);
            foreach (var receiver in receivers)
            {
                bufferedEvent.DeliveryStatuses.Add(new ReceiverDeliveryStatus { ReceiverName = receiver.Name });
            }
            newEvents.Add(bufferedEvent);
        }

        if (newEvents.Any())
        {
            await context.BufferedChangeEvents.AddRangeAsync(newEvents);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BufferedChangeEvent>> GetPendingChangesAsync(string receiverName, string trackingInstanceName, int batchSize)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        
        // Мощный и предсказуемый запрос EF Core
        var changes = await context.BufferedChangeEvents
            .Include(e => e.DeliveryStatuses)
            .Where(e => e.TrackingInstanceName == trackingInstanceName)
            .Where(e => e.DeliveryStatuses.Any(s => s.ReceiverName == receiverName && s.Status == DeliveryStatus.Pending))
            .OrderBy(e => e.BufferedAtUtc)
            .Take(batchSize)
            .ToListAsync();

        return changes;
    }

    public async Task UpdateChangeStatusAsync(Guid changeId, string trackingInstanceName, string receiverName, bool success, string? errorMessage)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var status = await context.ReceiverDeliveryStatuses
            .FirstOrDefaultAsync(s => s.BufferedChangeEventId == changeId && s.ReceiverName == receiverName);

        if (status != null)
        {
            status.Status = success ? DeliveryStatus.Success : DeliveryStatus.Failed;
            status.AttemptCount++;
            status.LastAttemptAtUtc = DateTime.UtcNow;
            status.ErrorDescription = errorMessage;
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<int> CleanupAsync(string trackingInstanceName, TimeSpan timeToLive)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var cutoffDate = DateTime.UtcNow - timeToLive;

        var eventsToDelete = await context.BufferedChangeEvents
            .Where(e => e.TrackingInstanceName == trackingInstanceName)
            .Where(e => e.BufferedAtUtc < cutoffDate)
            .Where(e => e.DeliveryStatuses.All(s => s.Status == DeliveryStatus.Success))
            .ToListAsync();

        if (eventsToDelete.Any())
        {
            context.BufferedChangeEvents.RemoveRange(eventsToDelete);
            return await context.SaveChangesAsync();
        }
        return 0;
    }
}