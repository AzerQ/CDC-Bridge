using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CdcBridge.Core.Models;

namespace CdcBridge.Persistence.Models;

/// <summary>
/// Представляет основной объект, хранящийся в базе данных.
/// Этот объект оборачивает исходное изменение (`TrackedChange`) и добавляет
/// метаданные, необходимые для надежной доставки и отслеживания состояния.
/// </summary>
public class BufferedChangeEvent
{
    /// <summary>
    /// Уникальный идентификатор события в базе данных.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Имя экземпляра отслеживания, от которого пришло это изменение.
    /// </summary>
    [Required]
    public string TrackingInstanceName { get; set; }

    /// <summary>
    /// Метка строки (LSN, timestamp и т.д.), используемая для отслеживания позиции.
    /// </summary>
    [Required]
    public string RowLabel { get; set; }

    /// <summary>
    /// Исходный объект изменения, полученный от `ICdcSource`.
    /// Будет храниться в базе данных как JSON.
    /// </summary>
    [Required]
    public TrackedChange Change { get; set; }

    /// <summary>
    /// Временная метка добавления события в буфер (в UTC). Используется для логики TTL.
    /// </summary>
    public DateTime BufferedAtUtc { get; set; }

    /// <summary>
    /// Коллекция статусов доставки этого события для каждого получателя.
    /// </summary>
    public virtual ICollection<ReceiverDeliveryStatus> DeliveryStatuses { get; set; } = new List<ReceiverDeliveryStatus>();
}