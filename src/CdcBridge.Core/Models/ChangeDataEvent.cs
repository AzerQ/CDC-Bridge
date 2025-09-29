using System;
using System.Text.Json.Nodes;

namespace CdcBridge.Core.Models;

/// <summary>
/// Модель события данных об изменении для журналирования и аудита.
/// Представляет полную информацию о событии изменения, включая результаты обработки и трансформации.
/// </summary>
public class ChangeDataEvent
{
    /// <summary>
    /// Уникальный идентификатор события.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Временная метка создания записи о событии.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Исходное отслеженное изменение.
    /// </summary>
    public required TrackedChange TrackedChange { get; set; }

    /// <summary>
    /// Трансформированные данные события после применения трансформера.
    /// Содержит данные в том виде, в котором они были отправлены получателю.
    /// </summary>
    public required JsonNode TransformedChange { get; set; }

    /// <summary>
    /// Идентификатор экземпляра отслеживания, к которому относится событие.
    /// </summary>
    public required string TrackingInstance { get; set; }

    /// <summary>
    /// Имя получателя, которому было отправлено событие.
    /// </summary>
    public required string ReceiverName { get; set; }

    /// <summary>
    /// Имя фильтра, который был применен к событию (если использовался).
    /// </summary>
    public string? FilterName { get; set; }

    /// <summary>
    /// Имя трансформера, который был применен к событию (если использовался).
    /// </summary>
    public string? TransformerName { get; set; }

    /// <summary>
    /// Результат обработки события получателем.
    /// Содержит информацию об успехе или ошибке отправки.
    /// </summary>
    public ReceiverProcessResult? ProcessResult { get; set; }

}
