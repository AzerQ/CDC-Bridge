using CdcBridge.Configuration.Models;
using System.Text.Json;

namespace CdcBridge.Configuration;

/// <summary>
/// Контекст конфигурации системы CDC Bridge.
/// Предоставляет доступ к настройкам системы и методам для работы с компонентами конфигурации.
/// </summary>
/// <remarks>
/// Контекст является центральной точкой доступа к конфигурации системы.
/// Обеспечивает согласованность данных и предоставляет методы для навигации по связанным компонентам.
/// Поддерживает механизм уведомлений об изменениях конфигурации.
/// </remarks>
public interface ICdcConfigurationContext
{
    /// <summary>
    /// Текущая конфигурация сервиса CDC
    /// </summary>
    public CdcSettings CdcSettings { get; }
    
    /// <summary>
    /// Получает настройки подключения по имени.
    /// </summary>
    /// <param name="name">Уникальное имя подключения.</param>
    /// <returns>
    /// Настройки подключения или <c>null</c>, если подключение с указанным именем не найдено.
    /// </returns>
    Connection? GetConnection(string name);

    /// <summary>
    /// Получает экземпляр отслеживания по имени.
    /// </summary>
    /// <param name="name">Уникальное имя экземпляра отслеживания.</param>
    /// <returns>
    /// Настройки экземпляра отслеживания или <c>null</c>, если экземпляр с указанным именем не найден.
    /// </returns>
    TrackingInstance? GetTrackingInstance(string name);

    /// <summary>
    /// Получает настройки фильтра по имени.
    /// </summary>
    /// <param name="name">Уникальное имя фильтра.</param>
    /// <returns>
    /// Настройки фильтра или <c>null</c>, если фильтр с указанным именем не найден.
    /// </returns>
    Filter? GetFilter(string name);

    /// <summary>
    /// Получает настройки трансформера по имени.
    /// </summary>
    /// <param name="name">Уникальное имя трансформера.</param>
    /// <returns>
    /// Настройки трансформера или <c>null</c>, если трансформер с указанным именем не найден.
    /// </returns>
    Transformer? GetTransformer(string name);

    /// <summary>
    /// Получает настройки получателя по имени.
    /// </summary>
    /// <param name="name">Уникальное имя получателя.</param>
    /// <returns>
    /// Настройки получателя или <c>null</c>, если получатель с указанным именем не найден.
    /// </returns>
    Receiver? GetReceiver(string name);

    /// <summary>
    /// Получает все экземпляры отслеживания для указанного подключения.
    /// </summary>
    /// <param name="connectionName">Имя подключения.</param>
    /// <returns>
    /// Коллекция экземпляров отслеживания, связанных с указанным подключением.
    /// Если подключение не найдено или не имеет связанных экземпляров, возвращается пустая коллекция.
    /// </returns>
    IReadOnlyList<TrackingInstance> GetTrackingInstancesForConnection(string connectionName);

    /// <summary>
    /// Получает все фильтры для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания.</param>
    /// <returns>
    /// Коллекция фильтров, связанных с указанным экземпляром отслеживания.
    /// Если экземпляр не найден или не имеет связанных фильтров, возвращается пустая коллекция.
    /// </returns>
    IReadOnlyList<Filter> GetFiltersForTrackingInstance(string trackingInstanceName);

    /// <summary>
    /// Получает все трансформеры для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания.</param>
    /// <returns>
    /// Коллекция трансформеров, связанных с указанным экземпляром отслеживания.
    /// Если экземпляр не найден или не имеет связанных трансформеров, возвращается пустая коллекция.
    /// </returns>
    IReadOnlyList<Transformer> GetTransformersForTrackingInstance(string trackingInstanceName);
    
    /// <summary>
    /// Получает все получатели для указанного экземпляра отслеживания.
    /// </summary>
    /// <param name="trackingInstanceName">Имя экземпляра отслеживания.</param>
    /// <returns>
    /// Коллекция получателей, связанных с указанным экземпляром отслеживания.
    /// Если экземпляр не найден или не имеет связанных получателей, возвращается пустая коллекция.
    /// </returns>
    IReadOnlyList<Receiver> GetReceiversForTrackingInstance(string trackingInstanceName);

    /// <summary>
    /// Получает полную цепочку обработки для указанного получателя.
    /// </summary>
    /// <param name="receiverName">Имя получателя.</param>
    /// <returns>
    /// Полная цепочка обработки, включающая получатель, фильтр и трансформер (если указаны).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Получатель с указанным именем не найден или ссылки на фильтр/трансформер невалидны.
    /// </exception>
    /// <example>
    /// <code>
    /// var pipeline = context.GetReceiverPipeline("AnalyticsChannel");
    /// // pipeline содержит Receiver, Filter и Transformer
    /// </code>
    /// </example>
    ReceiverPipeline GetReceiverPipeline(string receiverName);

    /// <summary>
    /// Десериализует параметры компонента в указанный тип.
    /// </summary>
    /// <typeparam name="T">Тип, в который десериализуются параметры.</typeparam>
    /// <param name="componentType">Тип компонента (для логирования и обработки ошибок).</param>
    /// <param name="parameters">JSON-элемент с параметрами.</param>
    /// <returns>
    /// Экземпляр типа <typeparamref name="T"/> или <c>null</c>, если десериализация не удалась.
    /// </returns>
    /// <exception cref="ArgumentNullException">componentType или parameters являются null.</exception>
    T? GetParameters<T>(string componentType, JsonElement parameters) where T : class;
}