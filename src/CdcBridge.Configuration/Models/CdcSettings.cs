namespace CdcBridge.Configuration.Models;


/// <summary>
/// Основная конфигурация системы CDC Bridge.
/// Содержит все необходимые настройки для функционирования системы отслеживания изменений.
/// </summary>
public class CdcSettings
{
    /// <summary>
    /// Коллекция подключений к источникам данных.
    /// Каждое подключение определяет способ доступа к базе данных или другому источнику изменений.
    /// </summary>
    public required IEnumerable<Connection> Connections { get; set; }

    /// <summary>
    /// Коллекция экземпляров отслеживания.
    /// Каждый экземпляр определяет конкретную таблицу и столбцы для мониторинга изменений.
    /// </summary>
    public required IEnumerable<TrackingInstance> TrackingInstances { get; set; }

    /// <summary>
    /// Коллекция получателей уведомлений о событиях.
    /// Каждый получатель определяет канал доставки уведомлений об изменениях.
    /// </summary>
    public required IEnumerable<Receiver> Receivers { get; set; }

    /// <summary>
    /// Коллекция фильтров для отбора событий изменений.
    /// Фильтры позволяют ограничить обработку только определенными типами изменений.
    /// </summary>
    public IEnumerable<Filter> Filters { get; set; } = [];

    /// <summary>
    /// Коллекция трансформеров для преобразования данных.
    /// Трансформеры позволяют изменить формат данных перед отправкой получателям.
    /// </summary>
    public IEnumerable<Transformer> Transformers { get; set; } = [];

    /// <summary>
    /// Объединить текущую конфигурацию  с новой конфигурацией CDC
    /// </summary>
    /// <param name="anotherSettings">Дополнительные настройки CDC</param>
    /// <returns>Новый объект конфигурации со всеми значениями из двух конфигураций</returns>
    public CdcSettings Merge(CdcSettings anotherSettings)
    {
        return new CdcSettings
        {
            Connections = Connections.Concat(anotherSettings.Connections),
            TrackingInstances = TrackingInstances.Concat(anotherSettings.TrackingInstances),
            Receivers = Receivers.Concat(anotherSettings.Receivers),
            Filters = Filters.Concat(anotherSettings.Filters),
            Transformers = Transformers.Concat(anotherSettings.Transformers)
        };
    }
    
}