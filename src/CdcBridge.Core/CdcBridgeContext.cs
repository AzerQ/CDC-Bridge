using System;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models.Configuration;

namespace CdcBridge.Core;

/// <summary>
/// Центральный контекст системы CDC Bridge, обеспечивающий регистрацию и управление компонентами.
/// Служит как контейнер для всех зарегистрированных источников данных, фильтров, трансформеров и получателей,
/// а также предоставляет доступ к конфигурации системы.
/// </summary>
/// <param name="cdcBridgeStorage">Хранилище данных для работы с конфигурацией и состоянием системы.</param>
public class CdcBridgeContext(ICdcBridgeStorage cdcBridgeStorage)
{
    #region Sources Management

    /// <summary>
    /// Коллекция зарегистрированных источников данных CDC.
    /// </summary>
    private readonly List<ICdcSource> sources = new();

    /// <summary>
    /// Регистрирует новый источник данных CDC в контексте.
    /// </summary>
    /// <param name="source">Источник данных для регистрации.</param>
    /// <returns>Текущий экземпляр контекста для цепочечного вызова методов.</returns>
    public CdcBridgeContext RegisterSource(ICdcSource source)
    {
        sources.Add(source);
        return this;
    }

    /// <summary>
    /// Получает все зарегистрированные источники данных CDC.
    /// </summary>
    /// <returns>Коллекция зарегистрированных источников данных.</returns>
    public IEnumerable<ICdcSource> GetSources() => sources;

    #endregion

    #region Filters Management

    /// <summary>
    /// Коллекция зарегистрированных фильтров событий.
    /// </summary>
    private readonly List<IFilter> filters = new();

    /// <summary>
    /// Регистрирует новый фильтр событий в контексте.
    /// </summary>
    /// <param name="filter">Фильтр для регистрации.</param>
    /// <returns>Текущий экземпляр контекста для цепочечного вызова методов.</returns>
    public CdcBridgeContext RegisterFilter(IFilter filter)
    {
        filters.Add(filter);
        return this;
    }

    /// <summary>
    /// Получает зарегистрированный фильтр по имени.
    /// </summary>
    /// <param name="filterName">Имя фильтра для поиска.</param>
    /// <returns>Фильтр с указанным именем.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если фильтр с указанным именем не найден.</exception>
    public IFilter GetFilter(string filterName) => filters.First(f => f.Name == filterName);

    #endregion

    #region Transformers Management

    /// <summary>
    /// Коллекция зарегистрированных трансформеров данных.
    /// </summary>
    private readonly List<ITransformer> transformers = new();

    /// <summary>
    /// Регистрирует новый трансформер данных в контексте.
    /// </summary>
    /// <param name="transformer">Трансформер для регистрации.</param>
    /// <returns>Текущий экземпляр контекста для цепочечного вызова методов.</returns>
    public CdcBridgeContext RegisterTransformer(ITransformer transformer)
    {
        transformers.Add(transformer);
        return this;
    }

    /// <summary>
    /// Получает зарегистрированный трансформер по имени.
    /// </summary>
    /// <param name="transformerName">Имя трансформера для поиска.</param>
    /// <returns>Трансформер с указанным именем.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если трансформер с указанным именем не найден.</exception>
    private ITransformer GetTransformer(string transformerName) => transformers.First(t => t.Name == transformerName);

    #endregion

    #region Receivers Management

    /// <summary>
    /// Коллекция зарегистрированных получателей уведомлений.
    /// </summary>
    private readonly List<IReceiver> receivers = new();

    /// <summary>
    /// Регистрирует новый получатель уведомлений в контексте.
    /// </summary>
    /// <param name="receiver">Получатель для регистрации.</param>
    /// <returns>Текущий экземпляр контекста для цепочечного вызова методов.</returns>
    public CdcBridgeContext RegisterReceiver(IReceiver receiver)
    {
        receivers.Add(receiver);
        return this;
    }

    /// <summary>
    /// Получает зарегистрированный получатель по имени.
    /// </summary>
    /// <param name="receiverName">Имя получателя для поиска.</param>
    /// <returns>Получатель с указанным именем.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если получатель с указанным именем не найден.</exception>
    public IReceiver GetReceiver(string receiverName) => receivers.First(r => r.Name == receiverName);

    #endregion

    #region Configuration Access

    /// <summary>
    /// Получает текущую конфигурацию системы CDC Bridge.
    /// </summary>
    /// <remarks>
    /// Это свойство выполняет синхронное получение конфигурации из хранилища.
    /// В производственном коде рекомендуется использовать асинхронные методы для избежания блокировки потока.
    /// </remarks>
    public CdcBridgeConfiguration Configuration => cdcBridgeStorage.GetConfiguration().Result;

    /// <summary>
    /// Получает настройки трансформера по имени из конфигурации.
    /// </summary>
    /// <param name="name">Имя трансформера.</param>
    /// <returns>Настройки трансформера или <c>null</c>, если трансформер не найден.</returns>
    public Transformer? GetTransformerSettings(string name) => Configuration.Transformers.FirstOrDefault(t => t.Name == name);

    /// <summary>
    /// Получает настройки подключения по имени из конфигурации.
    /// </summary>
    /// <param name="name">Имя подключения.</param>
    /// <returns>Настройки подключения или <c>null</c>, если подключение не найдено.</returns>
    public Connection? GetConnectionSettings(string name) => Configuration.Connections.FirstOrDefault(c => c.Name == name);

    /// <summary>
    /// Получает настройки получателя по имени из конфигурации.
    /// </summary>
    /// <param name="name">Имя получателя.</param>
    /// <returns>Настройки получателя или <c>null</c>, если получатель не найден.</returns>
    public Receiver? GetReceiverSettings(string name) => Configuration.Receivers.FirstOrDefault(r => r.Name == name);

    /// <summary>
    /// Получает настройки фильтра по имени из конфигурации.
    /// </summary>
    /// <param name="name">Имя фильтра.</param>
    /// <returns>Настройки фильтра или <c>null</c>, если фильтр не найден.</returns>
    public Filter? GetFilterSettings(string name) => Configuration.Filters.FirstOrDefault(f => f.Name == name);

    /// <summary>
    /// Получает настройки экземпляра отслеживания по имени таблицы из конфигурации.
    /// </summary>
    /// <param name="tableName">Имя исходной таблицы для поиска.</param>
    /// <returns>Настройки экземпляра отслеживания или <c>null</c>, если экземпляр не найден.</returns>
    public TrackingInstance? GetTrackingInstanceSettings(string tableName) => Configuration.TrackingInstances.FirstOrDefault(ti => ti.SourceTable == tableName);

    #endregion




}
