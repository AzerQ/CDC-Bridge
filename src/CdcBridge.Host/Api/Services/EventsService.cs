using CdcBridge.Host.Api.DTOs;
using CdcBridge.Persistence;
using CdcBridge.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// Сервис для работы с событиями изменений данных.
/// </summary>
public class EventsService : IEventsService
{
    private readonly IDbContextFactory<CdcBridgeDbContext> _dbContextFactory;

    public EventsService(IDbContextFactory<CdcBridgeDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Получает список событий с фильтрацией и пагинацией.
    /// </summary>
    public async Task<PagedResultDto<EventDto>> GetEventsAsync(EventQueryDto query)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var queryable = context.BufferedChangeEvents
            .Include(e => e.DeliveryStatuses)
            .AsQueryable();

        // Применяем фильтры
        if (!string.IsNullOrEmpty(query.TrackingInstanceName))
        {
            queryable = queryable.Where(e => e.TrackingInstanceName == query.TrackingInstanceName);
        }

        if (query.FromDate.HasValue)
        {
            queryable = queryable.Where(e => e.BufferedAtUtc >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            queryable = queryable.Where(e => e.BufferedAtUtc <= query.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(query.ReceiverName))
        {
            queryable = queryable.Where(e => e.DeliveryStatuses.Any(s => s.ReceiverName == query.ReceiverName));
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            var status = Enum.Parse<DeliveryStatus>(query.Status, true);
            queryable = queryable.Where(e => e.DeliveryStatuses.Any(s => s.ReceiverName == query.ReceiverName && s.Status == status));
        }

        var totalCount = await queryable.CountAsync();

        var events = await queryable
            .OrderByDescending(e => e.BufferedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var items = events.Select(e => new EventDto
        {
            Id = e.Id,
            TrackingInstanceName = e.TrackingInstanceName,
            RowLabel = e.RowLabel,
            BufferedAt = e.BufferedAtUtc,
            OperationType = e.Change?.ChangeType.ToString(),
            DeliveryStatuses = e.DeliveryStatuses.Select(s => new EventDeliveryStatusDto
            {
                ReceiverName = s.ReceiverName,
                Status = s.Status.ToString(),
                AttemptCount = s.AttemptCount,
                LastAttemptAt = s.LastAttemptAtUtc,
                ErrorDescription = s.ErrorDescription,
                LastDeliveryTimeMs = s.LastDeliveryTimeMs,
                AverageDeliveryTimeMs = s.AverageDeliveryTimeMs
            }).ToList()
        }).ToList();

        return new PagedResultDto<EventDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    /// <summary>
    /// Получает детальную информацию о конкретном событии.
    /// </summary>
    public async Task<EventDto?> GetEventByIdAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var bufferedEvent = await context.BufferedChangeEvents
            .Include(e => e.DeliveryStatuses)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (bufferedEvent == null)
        {
            return null;
        }

        return new EventDto
        {
            Id = bufferedEvent.Id,
            TrackingInstanceName = bufferedEvent.TrackingInstanceName,
            RowLabel = bufferedEvent.RowLabel,
            BufferedAt = bufferedEvent.BufferedAtUtc,
            OperationType = bufferedEvent.Change?.ChangeType.ToString(),
            DeliveryStatuses = bufferedEvent.DeliveryStatuses.Select(s => new EventDeliveryStatusDto
            {
                ReceiverName = s.ReceiverName,
                Status = s.Status.ToString(),
                AttemptCount = s.AttemptCount,
                LastAttemptAt = s.LastAttemptAtUtc,
                ErrorDescription = s.ErrorDescription,
                LastDeliveryTimeMs = s.LastDeliveryTimeMs,
                AverageDeliveryTimeMs = s.AverageDeliveryTimeMs
            }).ToList()
        };
    }
}
