using System;
using CdcBridge.Core.Models.Configuration;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для управления жизненным циклом экземпляров отслеживания.
/// Определяет контракт для запуска, остановки и проверки состояния отслеживания изменений.
/// </summary>
public interface ITrackingInstanceService
{
    /// <summary>
    /// Запускает отслеживание изменений для указанного экземпляра.
    /// Инициализирует мониторинг указанной таблицы и столбцов.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания для запуска.</param>
    public Task StartTracking(TrackingInstance trackingInstance);

    /// <summary>
    /// Останавливает отслеживание изменений для указанного экземпляра.
    /// Прекращает мониторинг и освобождает связанные ресурсы.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания для остановки.</param>
    public Task StopTracking(TrackingInstance trackingInstance);

    /// <summary>
    /// Проверяет, активно ли отслеживание для указанного экземпляра.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания для проверки.</param>
    /// <returns><c>true</c>, если отслеживание активно; иначе <c>false</c>.</returns>
    public Task<bool> IsTracking(TrackingInstance trackingInstance);
    
}
