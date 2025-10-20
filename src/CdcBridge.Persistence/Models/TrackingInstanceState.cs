using System.ComponentModel.DataAnnotations;

namespace CdcBridge.Persistence.Models;

/// <summary>
/// Представляет объект для хранения состояния обработки источника данных.
/// Хранит "курсор" или "указатель", с которого нужно продолжать чтение изменений.
/// </summary>
public class TrackingInstanceState
{
    /// <summary>
    /// Уникальное имя экземпляра отслеживания, используемое как первичный ключ.
    /// </summary>
    [Key]
    public string TrackingInstanceName { get; set; }

    /// <summary>
    /// Метка последней успешно зафиксированной в буфере строки из источника (например, LSN).
    /// </summary>
    public string? LastProcessedRowLabel { get; set; }

    /// <summary>
    /// Временная метка последнего обновления этого состояния в UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}