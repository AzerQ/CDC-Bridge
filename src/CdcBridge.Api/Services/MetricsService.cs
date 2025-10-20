using CdcBridge.Api.DTOs;
using CdcBridge.Persistence;
using CdcBridge.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CdcBridge.Api.Services;

/// <summary>
/// Сервис для получения метрик системы CDC Bridge.
/// </summary>
public class MetricsService
{
    private readonly IDbContextFactory<CdcBridgeDbContext> _dbContextFactory;

    public MetricsService(IDbContextFactory<CdcBridgeDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Получает общие метрики системы.
    /// </summary>
    public async Task<MetricsDto> GetMetricsAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var totalBuffered = await context.BufferedChangeEvents.CountAsync();
        var allStatuses = await context.ReceiverDeliveryStatuses.ToListAsync();

        var pendingCount = allStatuses.Count(s => s.Status == DeliveryStatus.Pending);
        var successCount = allStatuses.Count(s => s.Status == DeliveryStatus.Success);
        var failedCount = allStatuses.Count(s => s.Status == DeliveryStatus.Failed);

        var avgDeliveryTime = allStatuses
            .Where(s => s.AverageDeliveryTimeMs.HasValue)
            .Select(s => s.AverageDeliveryTimeMs!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var receiverMetrics = allStatuses
            .GroupBy(s => s.ReceiverName)
            .Select(g => new ReceiverMetricsDto
            {
                ReceiverName = g.Key,
                PendingCount = g.Count(s => s.Status == DeliveryStatus.Pending),
                SuccessCount = g.Count(s => s.Status == DeliveryStatus.Success),
                FailedCount = g.Count(s => s.Status == DeliveryStatus.Failed),
                AverageDeliveryTimeMs = g
                    .Where(s => s.AverageDeliveryTimeMs.HasValue)
                    .Select(s => s.AverageDeliveryTimeMs!.Value)
                    .DefaultIfEmpty(0)
                    .Average(),
                LastAttemptAt = g.Max(s => s.LastAttemptAtUtc)
            })
            .ToList();

        return new MetricsDto
        {
            TotalBufferedEvents = totalBuffered,
            PendingEvents = pendingCount,
            SuccessfulEvents = successCount,
            FailedEvents = failedCount,
            AverageDeliveryTimeMs = avgDeliveryTime > 0 ? avgDeliveryTime : null,
            ReceiverMetrics = receiverMetrics
        };
    }
}
