namespace CdcBridge.Persistence.Models;

/// <summary>
/// Представляет статус доставки одного события одному конкретному получателю.
/// </summary>
public class ReceiverStatus
{
    /// <summary>
    /// Текущий статус доставки.
    /// </summary>
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

    /// <summary>
    /// Количество попыток отправки.
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Временная метка последней попытки отправки в UTC.
    /// </summary>
    public DateTime? LastAttemptAtUtc { get; set; }

    /// <summary>
    /// Описание ошибки, если последняя попытка была неудачной.
    /// </summary>
    public string? ErrorDescription { get; set; }
}