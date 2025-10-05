using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CdcBridge.Configuration;
using CdcBridge.Core.Abstractions;
using CdcBridge.Application.CdcSources;
using CdcBridge.Application.Filters;
using CdcBridge.Application.Transformers;
using CdcBridge.Application.Receivers.CdcWebhookReceiver;
using CdcBridge.Persistence;
using CdcBridge.Persistence.Abstractions;
using CdcBridge.Service;
using CdcBridge.Service.Workers;
using Microsoft.Extensions.Logging;

namespace CdcBridge.Application.DI;

/// <summary>
/// Содержит методы расширения для IServiceCollection для полной настройки и регистрации
/// всех сервисов, необходимых для работы CDC Bridge.
/// </summary>
public static class CdcBridgeServiceCollectionExtensions
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
    
    /// <summary>
    /// Регистрирует все стандартные сервисы CDC Bridge "из коробки" одним вызовом.
    /// Включает в себя ядро, персистентность, стандартные компоненты и фоновые службы.
    /// </summary>
    /// <param name="services">Коллекция сервисов DI.</param>
    /// <param name="configuration">Конфигурация приложения для получения настроек.</param>
    /// <returns>Та же коллекция сервисов для возможности цепочечного вызова.</returns>
    public static IServiceCollection AddCdcBridge(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCdcBridgeCoreServices(configuration)
            .AddCdcBridgePersistence(configuration)
            .AddCdcBridgeApplicationComponents()
            .AddCdcBridgeHostedServices();

        return services;
    }

    /// <summary>
    /// Регистрирует основные службы ядра CDC Bridge, такие как контекст конфигурации и фабрику компонентов.
    /// </summary>
    private static IServiceCollection AddCdcBridgeCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICdcConfigurationContext>(sp =>
        {
            var configPath = configuration.GetValue<string>("CdcBridge:ConfigurationPath");
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new FileNotFoundException("CDC Bridge configuration YAML file not found.", configPath);
            }

            return new CdcConfigurationContextBuilder()
                .AddConfigurationFromFile(configPath)
                .Build();
        });

        services.AddSingleton<ComponentFactory>();
        return services;
    }

    /// <summary>
    /// Регистрирует слой персистентности на базе LiteDB.
    /// </summary>
    private static IServiceCollection AddCdcBridgePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Используем метод расширения из самого слоя Persistence
        services.AddLiteDbPersistence(configuration);
        return services;
    }

    /// <summary>
    /// Регистрирует все стандартные реализации компонентов (источники, фильтры, трансформеры, получатели).
    /// </summary>
    private static IServiceCollection AddCdcBridgeApplicationComponents(this IServiceCollection services)
    {
        // Источники (ICdcSource)
        services.AddTransient<SqlServerCdcSource>();
        services.AddTransient<ICdcSource, SqlServerCdcSource>(s => s.GetRequiredService<SqlServerCdcSource>());

        // Фильтры (IFilter)
        services.AddTransient<JsonPathFilter>();
        services.AddTransient<IFilter, JsonPathFilter>(s => s.GetRequiredService<JsonPathFilter>());

        // Трансформеры (ITransformer)
        services.AddTransient<JSONataTransformer>();
        services.AddTransient<ITransformer, JSONataTransformer>(s => s.GetRequiredService<JSONataTransformer>());

        // Получатели (IReceiver)
        services.AddHttpClient<WebhookReceiver>();
        services.AddTransient<WebhookReceiver>();
        services.AddTransient<IReceiver, WebhookReceiver>(s => s.GetRequiredService<WebhookReceiver>());
        
        return services;
    }

    /// <summary>
    /// Регистрирует основные фоновые службы (Hosted Services), которые составляют ядро выполнения.
    /// </summary>
    private static IServiceCollection AddCdcBridgeHostedServices(this IServiceCollection services)
    {
        // Главный дирижер, запускающий воркеры
        services.AddHostedService<CdcBridgeOrchestrator>();

        // Фоновый сервис для очистки буфера
        services.AddHostedService<CleanupWorker>();
        
        return services;
    }
}