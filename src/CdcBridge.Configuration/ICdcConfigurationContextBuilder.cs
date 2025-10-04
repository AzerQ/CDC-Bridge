using CdcBridge.Configuration.Models;

namespace CdcBridge.Configuration;

/// <summary>
/// Построитель контекста конфигурации системы CDC Bridge.
/// Предоставляет fluent-интерфейс для пошагового построения конфигурации из различных источников.
/// </summary>
/// <remarks>
/// Реализует паттерн Builder для создания иммутабельного контекста конфигурации.
/// Поддерживает загрузку конфигурации из файлов, строкового содержимого и готовых объектов.
/// </remarks>
/// <example>
/// <code>
/// var context = new ConfigurationContextBuilder()
///     .AddConfigurationFromFile("config.yaml")
///     .AddConfigurationFromString("connections: [...]")
///     .Build();
/// </code>
/// </example>
public interface ICdcConfigurationContextBuilder
{
    /// <summary>
    /// Добавляет конфигурацию из YAML файла.
    /// </summary>
    /// <param name="filePath">Путь к YAML файлу конфигурации.</param>
    /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
    /// <exception cref="FileNotFoundException">Указанный файл не существует.</exception>
    /// <exception cref="CdcBridge.Configuration.CdcConfigurationLoadException">Файл содержит некорректную YAML структуру.</exception>
    ICdcConfigurationContextBuilder AddConfigurationFromFile(string filePath);

    /// <summary>
    /// Добавляет конфигурацию из строки с YAML содержимым.
    /// </summary>
    /// <param name="yamlContent">YAML содержимое конфигурации.</param>
    /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
    /// <exception cref="ArgumentNullException">yamlContent является null или пустой строкой.</exception>
    /// <exception cref="CdcBridge.Configuration.CdcConfigurationLoadException">Строка содержит некорректную YAML структуру.</exception>
    ICdcConfigurationContextBuilder AddConfigurationFromString(string yamlContent);

    /// <summary>
    /// Добавляет готовый объект настроек в конфигурацию.
    /// </summary>
    /// <param name="newSettings">Объект настроек для добавления.</param>
    /// <returns>Текущий экземпляр построителя для цепочки вызовов.</returns>
    /// <exception cref="ArgumentNullException">newSettings является null.</exception>
    /// <exception cref="CdcBridge.Configuration.CdcConfigurationLoadException">Невалидный объект конфигурации</exception>
    ICdcConfigurationContextBuilder AddConfiguration(CdcSettings newSettings);

    /// <summary>
    /// Создает иммутабельный контекст конфигурации на основе добавленных настроек.
    /// </summary>
    /// <returns>Готовый к использованию контекст конфигурации.</returns>
    /// <exception cref="InvalidOperationException">
    /// Не добавлено ни одной конфигурации или конфигурация содержит ошибки валидации.
    /// </exception>
    ICdcConfigurationContext Build();
}