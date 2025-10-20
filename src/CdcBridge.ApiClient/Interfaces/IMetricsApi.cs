using Refit;

namespace CdcBridge.ApiClient.Interfaces;

/// <summary>
/// Refit interface для работы с API метрик.
/// </summary>
public interface IMetricsApi
{
    /// <summary>
    /// Получает метрики системы.
    /// </summary>
    [Get("/api/metrics")]
    [Headers("Authorization: Bearer")]
    Task<MetricsDto> GetMetricsAsync();
}

/// <summary>
/// DTO для метрик (совпадает с серверным DTO).
/// </summary>
public class MetricsDto
{
    public int TotalBufferedEvents { get; set; }
    public int PendingEvents { get; set; }
    public int SuccessfulEvents { get; set; }
    public int FailedEvents { get; set; }
    public double? AverageDeliveryTimeMs { get; set; }
    public List<ReceiverMetricsDto> ReceiverMetrics { get; set; } = new();
}

public class ReceiverMetricsDto
{
    public required string ReceiverName { get; set; }
    public int PendingCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public double? AverageDeliveryTimeMs { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}
