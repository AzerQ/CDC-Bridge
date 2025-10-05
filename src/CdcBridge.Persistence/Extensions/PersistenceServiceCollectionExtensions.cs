using CdcBridge.Configuration;
using CdcBridge.Persistence.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CdcBridge.Persistence.Extensions;

/// <summary>
/// Содержит методы расширения для регистрации сервисов слоя персистентности в DI-контейнере.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует реализацию хранилища на базе LiteDB и все его зависимости в DI-контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов DI.</param>
    /// <param name="configuration">Конфигурация приложения для получения настроек.</param>
    /// <returns>Та же коллекция сервисов для возможности цепочечного вызова.</returns>
    public static IServiceCollection AddLiteDbPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Получаем путь к файлу БД из конфигурации.
        // Если путь не указан, используем значение по умолчанию "cdc_bridge.db".
        // Это делает конфигурацию гибкой, но не требует ее в обязательном порядке.
        string dbPath = configuration.GetValue<string>("Persistence:LiteDbPath") ?? "cdc_bridge.db";

        // Регистрируем нашу реализацию ICdcBridgeStorage как Singleton.
        // LiteDB-Async потокобезопасна, поэтому Singleton - подходящее время жизни.
        services.AddSingleton<ICdcBridgeStorage>(sp => 
        {
            // Разрешаем зависимости, необходимые для конструктора LiteDbAsyncStorage
            var configContext = sp.GetRequiredService<ICdcConfigurationContext>();
            var logger = sp.GetRequiredService<ILogger<LiteDbAsyncStorage>>();
            
            // Создаем и возвращаем экземпляр нашего хранилища
            return new LiteDbAsyncStorage(dbPath, configContext, logger);
        });

        return services;
    }
}