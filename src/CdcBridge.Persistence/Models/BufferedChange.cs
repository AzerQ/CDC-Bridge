using LiteDB;
using CdcBridge.Core.Models;

namespace CdcBridge.Persistence.Models;

/// <summary>
/// Перечисление возможных статусов доставки события.
/// </summary>
public enum DeliveryStatus { Pending, Success, Failed }

/// <summary>
/// Представляет основной документ, хранящийся в LiteDB.
/// Этот объект оборачивает исходное изменение (`TrackedChange`) и добавляет
/// метаданные, необходимые для надежной доставки и отслеживания состояния.
/// </summary>
public class BufferedChange
{
    /// <summary>
    /// Уникальный идентификатор документа в LiteDB.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    /// Исходный объект изменения, полученный от `ICdcSource`.
    /// </summary>
    public required TrackedChange Change { get; set; }

    /// <summary>
    /// Временная метка добавления события в буфер (в UTC). Используется для логики TTL (Time-To-Live).
    /// </summary>
    public DateTime BufferedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Словарь, отслеживающий статусы доставки этого события для каждого получателя.
    /// Ключ - уникальное имя получателя (`Receiver.Name`).
    /// Значение - объект `ReceiverStatus`, содержащий состояние доставки.
    /// </summary>
    public Dictionary<string, ReceiverStatus> DeliveryStatuses { get; set; } = new();
}