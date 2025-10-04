using CdcBridge.Configuration.Models;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для хранения данных системы CDC Bridge.
/// Обеспечивает доступ к конфигурации, состоянию отслеживания и журналированию событий.
/// </summary>
public interface ICdcBridgeStorage
{
    
    /// <summary>
    /// Получает номер последней обработанной строки для указанного экземпляра отслеживания.
    /// Используется для определения точки, с которой необходимо продолжить чтение изменений.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания, для которого требуется получить позицию.</param>
    /// <returns>Номер последней обработанной строки или <c>null</c>, если обработка еще не начиналась.</returns>
    public Task<string?> GetLastProcessedRowNumber(TrackingInstance trackingInstance);

    /// <summary>
    /// Сохраняет номер последней обработанной строки для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания.</param>
    /// <param name="lastRowNumber">Номер последней обработанной строки.</param>
    public Task SaveLastProcessedRowNumber(TrackingInstance trackingInstance, string lastRowNumber);

    /// <summary>
    /// Добавляет записи о событиях изменений в журнал.
    /// </summary>
    /// <param name="changes">Массив событий изменений для записи в журнал.</param>
    public Task AddChangeDataEventsLogs(params ChangeDataEvent[] changes);

    /// <summary>
    /// Обновляет существующие записи о событиях изменений в журнале.
    /// Обычно используется для обновления результатов обработки получателями.
    /// </summary>
    /// <param name="changes">Массив событий изменений для обновления.</param>
    public Task UpdateChangeDataEventsLogs(params ChangeDataEvent[] changes);

    /// <summary>
    /// Получает записи журнала событий изменений для указанного экземпляра отслеживания в заданном временном диапазоне.
    /// </summary>
    /// <param name="trackingInstance">Экземпляр отслеживания.</param>
    /// <param name="from">Начальная дата (включительно).</param>
    /// <param name="to">Конечная дата (включительно).</param>
    /// <returns>Коллекция событий изменений.</returns>
    public Task<IEnumerable<ChangeDataEvent>> GetChangeDataEventsLogs(TrackingInstance trackingInstance, DateTime from, DateTime to);

    /// <summary>
    /// Очищает устаревшие записи журнала событий изменений.
    /// </summary>
    /// <param name="olderThan">Дата, старше которой записи будут удалены.</param>
    /// <returns>Количество удаленных записей.</returns>
    public Task<int> CleanupChangeDataEventsLogs(DateTime olderThan);

}
