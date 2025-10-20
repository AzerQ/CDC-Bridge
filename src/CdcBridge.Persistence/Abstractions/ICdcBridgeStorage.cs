using CdcBridge.Core.Models;
using CdcBridge.Persistence.Models;

namespace CdcBridge.Persistence.Abstractions;

/// <summary>
/// Определяет контракт для персистентного хранилища CDC Bridge.
/// Отвечает за буферизацию изменений, отслеживание состояния обработки для
/// источников и получателей, а также за очистку устаревших данных.
/// </summary>
public interface ICdcBridgeStorage
{
    Task<string?> GetLastProcessedRowLabelAsync(string trackingInstanceName);
    Task SaveLastProcessedRowLabelAsync(string trackingInstanceName, string rowLabel);

    /// <summary>
    /// Асинхронно добавляет коллекцию отслеженных изменений в буфер для последующей обработки.
    /// </summary>
    /// <param name="changes">Коллекция изменений, полученных от источника данных.</param>
    Task AddChangesToBufferAsync(IEnumerable<TrackedChange> changes);

    /// <summary>
    /// Асинхронно получает пачку необработанных (ожидающих) изменений для конкретного получателя.
    /// </summary>
    /// <param name="receiverName">Уникальное имя получателя.</param>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания, из которого нужно получить изменения.</param>
    /// <param name="batchSize">Максимальное количество изменений для получения.</param>
    /// <returns>Коллекция буферизованных изменений, ожидающих обработки данным получателем.</returns>
    Task<IEnumerable<BufferedChangeEvent>> GetPendingChangesAsync(string receiverName, string trackingInstanceName, int batchSize);

    /// <summary>
    /// Асинхронно обновляет статус доставки для одного буферизованного изменения и одного получателя.
    /// </summary>
    /// <param name="changeId">Уникальный идентификатор буферизованного изменения (теперь Guid).</param>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания, к которому относится изменение.</param>
    /// <param name="receiverName">Уникальное имя получателя.</param>
    /// <param name="success">True, если доставка была успешной, иначе false.</param>
    /// <param name="errorMessage">Сообщение об ошибке, если доставка не удалась.</param>
    /// <param name="deliveryTimeMs">Время доставки в миллисекундах.</param>
    Task UpdateChangeStatusAsync(Guid changeId, string trackingInstanceName, string receiverName, bool success, string? errorMessage, long? deliveryTimeMs = null);

    /// <summary>
    /// Асинхронно выполняет очистку хранилища, удаляя старые события, которые были успешно доставлены всем получателям.
    /// </summary>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания, для которого выполняется очистка.</param>
    /// <param name="timeToLive">Максимальное время жизни для успешно обработанного события.</param>
    /// <returns>Количество удаленных записей.</returns>
    Task<int> CleanupAsync(string trackingInstanceName, TimeSpan timeToLive);
}