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
            Connections = Connections.ConcatDistinctByField(anotherSettings.Connections, con => con.Name),
            TrackingInstances = TrackingInstances.ConcatDistinctByField(anotherSettings.TrackingInstances, t => t.Name),
            Receivers = Receivers.ConcatDistinctByField(anotherSettings.Receivers, r => r.Name),
            Filters = Filters.ConcatDistinctByField(anotherSettings.Filters, f => f.Name),
            Transformers = Transformers.ConcatDistinctByField(anotherSettings.Transformers, t => t.Name)
        };
    }
    
}

static class EnumerableExtensions
{
    public static IEnumerable<TElement> ConcatDistinctByField<TElement, TProperty>(this IEnumerable<TElement> source,
        IEnumerable<TElement> anotherCollection,
        Func<TElement, TProperty> selector)
    {
        var uniqueInAnotherCollection = anotherCollection
            .Where(anotherItem => !source.Any(item => selector(item)!.Equals(selector(anotherItem))));
        return source.Concat(uniqueInAnotherCollection);
    }
}