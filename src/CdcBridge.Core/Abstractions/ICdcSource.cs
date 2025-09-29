using System;
using CdcBridge.Core.Models;
using CdcBridge.Core.Models.Configuration;

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
    /// <param name="trackingInstance">Экземпляр отслеживания, для которого требуется получить изменения.</param>
    /// <param name="lastRowNumber">Номер последней обработанной строки. Изменения будут получены начиная с этой позиции.</param>
    /// <returns>Коллекция отслеженных изменений.</returns>
    public Task<IEnumerable<TrackedChange>> GetChanges(TrackingInstance trackingInstance, string lastRowNumber);

    /// <summary>
    /// Проверяет, включен ли CDC (Change Data Capture) для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания для проверки.</param>
    /// <returns><c>true</c>, если CDC включен; иначе <c>false</c>.</returns>
    public Task<bool> CheckCdcIsEnabled(TrackingInstance trackingInstance);

    /// <summary>
    /// Добавляет новый экземпляр отслеживания в источник данных.
    /// Настраивает CDC для указанной таблицы и столбцов.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания для добавления.</param>
    public Task AddTrackingInstance(TrackingInstance trackingInstance);

}
