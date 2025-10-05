using LiteDB;
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
    /// <summary>
    /// Асинхронно получает последнюю обработанную метку строки (например, LSN) для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstanceName">Уникальное имя экземпляра отслеживания.</param>
    /// <returns>Строковое представление метки последней обработанной строки или null, если обработка еще не начиналась.</returns>
    Task<string?> GetLastProcessedRowLabelAsync(string trackingInstanceName);

    /// <summary>
    /// Асинхронно сохраняет метку последней обработанной строки для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstanceName">Уникальное имя экземпляра отслеживания.</param>
    /// <param name="rowLabel">Строковое представление метки для сохранения.</param>
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
    Task<IEnumerable<BufferedChange>> GetPendingChangesAsync(string receiverName, string trackingInstanceName, int batchSize);

    /// <summary>
    /// Асинхронно обновляет статус доставки для одного буферизованного изменения и одного получателя.
    /// </summary>
    /// <param name="changeId">Уникальный идентификатор буферизованного изменения (из LiteDB).</param>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания, к которому относится изменение.</param>
    /// <param name="receiverName">Уникальное имя получателя.</param>
    /// <param name="success">True, если доставка была успешной, иначе false.</param>
    /// <param name="errorMessage">Сообщение об ошибке, если доставка не удалась.</param>
    Task UpdateChangeStatusAsync(ObjectId changeId, string trackingInstanceName, string receiverName, bool success, string? errorMessage);

    /// <summary>
    /// Асинхронно выполняет очистку хранилища, удаляя старые события, которые были успешно доставлены всем получателям.
    /// </summary>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания, для которого выполняется очистка.</param>
    /// <param name="timeToLive">Максимальное время жизни для успешно обработанного события.</param>
    /// <returns>Количество удаленных записей.</returns>
    Task<int> CleanupAsync(string trackingInstanceName, TimeSpan timeToLive);
}