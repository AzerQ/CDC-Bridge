using Refit;

namespace CdcBridge.ApiClient.Interfaces;

/// <summary>
/// Refit interface для работы с Events API.
/// </summary>
public interface IEventsApi
{
    /// <summary>
    /// Получает список событий с фильтрацией и пагинацией.
    /// </summary>
    [Get("/api/events")]
    [Headers("Authorization: Bearer")]
    Task<PagedResultDto<EventDto>> GetEventsAsync([Query] EventQueryDto query);

    /// <summary>
    /// Получает детальную информацию о конкретном событии.
    /// </summary>
    [Get("/api/events/{id}")]
    [Headers("Authorization: Bearer")]
    Task<EventDto?> GetEventByIdAsync(Guid id);
}

/// <summary>
/// DTO для отображения события изменения данных.
/// </summary>
public class EventDto
{
    public Guid Id { get; set; }
    public required string TrackingInstanceName { get; set; }
    public required string RowLabel { get; set; }
    public DateTime BufferedAt { get; set; }
    public string? OperationType { get; set; }
    public List<EventDeliveryStatusDto> DeliveryStatuses { get; set; } = new();
}

/// <summary>
/// DTO для статуса доставки события конкретному получателю.
/// </summary>
public class EventDeliveryStatusDto
{
    public required string ReceiverName { get; set; }
    public required string Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? ErrorDescription { get; set; }
    public long? LastDeliveryTimeMs { get; set; }
    public double? AverageDeliveryTimeMs { get; set; }
}

/// <summary>
/// Параметры запроса для получения списка событий с фильтрацией и пагинацией.
/// </summary>
public class EventQueryDto
{
    public string? TrackingInstanceName { get; set; }
    public string? ReceiverName { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
