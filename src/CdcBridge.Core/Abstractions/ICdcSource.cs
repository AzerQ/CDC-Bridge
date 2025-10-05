using CdcBridge.Configuration.Models;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для источника данных об изменениях (CDC).
/// Определяет контракт для получения изменений из различных типов источников данных.
/// </summary>
public interface ICdcSource
{
    /// <summary>
    /// Получает изменения для указанного экземпляра отслеживания, начиная с указанной позиции.
    /// </summary>
    /// <param name="trackingInstanceInfo">Экземпляр отслеживания, для которого требуется получить изменения.</param>
    /// <param name="cdcRequest">Дополнительные данные на запрос изменений</param>
    /// <returns>Коллекция отслеженных изменений.</returns>
    public Task<IEnumerable<TrackedChange>> GetChanges(TrackingInstanceInfo trackingInstanceInfo, CdcRequest? cdcRequest = null);

    /// <summary>
    /// Проверяет, включен ли CDC (Change Data Capture) для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstanceInfo">Экземпляр отслеживания для проверки.</param>
    /// <returns><c>true</c>, если CDC включен; иначе <c>false</c>.</returns>
    public Task<(bool isEnabled, string? message, string? dbTrackingInstanceName)> CheckCdcIsEnabled(TrackingInstanceInfo trackingInstanceInfo);

    /// <summary>
    /// Добавляет новый экземпляр отслеживания в источник данных.
    /// Настраивает CDC для указанной таблицы и столбцов.
    /// </summary>
    /// <param name="trackingInstanceInfo">Экземпляр отслеживания для добавления.</param>
    public Task EnableTrackingInstance(TrackingInstanceInfo trackingInstanceInfo);

    /// <summary>
    /// Добавляет новый экземпляр отслеживания в источник данных.
    /// Настраивает CDC для указанной таблицы и столбцов.
    /// </summary>
    /// <param name="trackingInstanceInfo">Экземпляр отслеживания для добавления.</param>
    public Task DisableTrackingInstance(TrackingInstanceInfo trackingInstanceInfo);

}

/// <summary>
/// Информация об экземпляре отслеживания вместе с подключением
/// </summary>
/// <param name="TrackingInstance">Экземпляр отслеживания привязанный к исходной таблице</param>
/// <param name="Connection">Информация о подключении к БД</param>
public record TrackingInstanceInfo(TrackingInstance TrackingInstance, Connection Connection);