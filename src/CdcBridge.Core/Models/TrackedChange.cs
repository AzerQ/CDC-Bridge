using System;

namespace CdcBridge.Core.Models;

/// <summary>
/// Модель зафиксированного изменения (CDC-событие).
/// Содержит тип изменения, идентификатор отслеживаемого источника, момент создания и данные «до/после».
/// </summary>
public class TrackedChange
{
    /// <summary>
    /// Тип изменения: вставка, обновление или удаление.
    /// </summary>
    public ChangeType ChangeType { get; set; }


    /// <summary>
    /// Идентификатор отслеживаемого источника, к которому относится событие.
    /// Рекомендуется использовать значение <c>TrackingInstance.sourceTable</c> из конфигурации.
    /// </summary>
    public required string TrackingInstance { get; set; }

    /// <summary>
    /// Временная метка создания события изменения в UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Данные «до/после» изменения. Для <see cref="ChangeType.Insert"/> поле <c>Old</c> обычно отсутствует, для
    /// <see cref="ChangeType.Delete"/> — отсутствует <c>New</c>, для <see cref="ChangeType.Update"/> присутствуют оба.
    /// </summary>
    public ChangeData? Data { get; set; }

    /// <example>
    /// Пример события обновления пользователя:
    /// <code language="json">
    /// {
    ///   "changeType": "Update",
    ///   "trackingInstance": "users",
    ///   "createdAt": "2025-09-29T12:34:56Z",
    ///   "data": {
    ///     "old": { "email": "old@example.com", "status": "active" },
    ///     "new": { "email": "new@example.com", "status": "inactive" }
    ///   }
    /// }
    /// </code>
    /// </example>
}
