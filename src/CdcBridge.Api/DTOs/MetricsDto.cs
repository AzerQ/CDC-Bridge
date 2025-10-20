namespace CdcBridge.Api.DTOs;

/// <summary>
/// DTO для отображения метрик системы CDC Bridge.
/// </summary>
public class MetricsDto
{
    /// <summary>
    /// Общее количество событий в буфере.
    /// </summary>
    public int TotalBufferedEvents { get; set; }

    /// <summary>
    /// Количество событий, ожидающих обработки.
    /// </summary>
    public int PendingEvents { get; set; }

    /// <summary>
    /// Количество успешно обработанных событий.
    /// </summary>
    public int SuccessfulEvents { get; set; }

    /// <summary>
    /// Количество неудачных событий.
    /// </summary>
    public int FailedEvents { get; set; }

    /// <summary>
    /// Среднее время доставки сообщений в миллисекундах.
    /// </summary>
    public double? AverageDeliveryTimeMs { get; set; }

    /// <summary>
    /// Метрики по каждому получателю.
    /// </summary>
    public List<ReceiverMetricsDto> ReceiverMetrics { get; set; } = new();
}

/// <summary>
/// DTO для метрик конкретного получателя.
/// </summary>
public class ReceiverMetricsDto
{
    /// <summary>
    /// Имя получателя.
    /// </summary>
    public required string ReceiverName { get; set; }

    /// <summary>
    /// Количество событий, ожидающих доставки.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Количество успешно доставленных событий.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Количество неудачных попыток доставки.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Среднее время доставки для этого получателя в миллисекундах.
    /// </summary>
    public double? AverageDeliveryTimeMs { get; set; }

    /// <summary>
    /// Последнее время попытки доставки.
    /// </summary>
    public DateTime? LastAttemptAt { get; set; }
}
