using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CdcBridge.Persistence.Models;

/// <summary>
/// Перечисление возможных статусов доставки события.
/// </summary>
public enum DeliveryStatus { Pending, Success, Failed }

/// <summary>
/// Представляет статус доставки одного события (`BufferedChangeEvent`)
/// одному конкретному получателю.
/// </summary>
public class ReceiverDeliveryStatus
{
    /// <summary>
    /// Уникальный идентификатор записи статуса.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Внешний ключ для связи с `BufferedChangeEvent`.
    /// </summary>
    [Required]
    public Guid BufferedChangeEventId { get; set; }

    /// <summary>
    /// Навигационное свойство к родительскому событию.
    /// </summary>
    [ForeignKey(nameof(BufferedChangeEventId))]
    public virtual BufferedChangeEvent BufferedChangeEvent { get; set; }

    /// <summary>
    /// Уникальное имя получателя, для которого предназначен этот статус.
    /// </summary>
    [Required]
    public string ReceiverName { get; set; }

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