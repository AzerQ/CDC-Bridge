using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CdcBridge.Logging;

/// <summary>
/// Методы расширения для настройки структурированного логирования в SQLite.
/// </summary>
public static class StructuredLoggingExtensions
{
    /// <summary>
    /// Добавляет структурированное логирование с хранением в SQLite.
    /// Настройки читаются из конфигурации (appsettings.json).
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Получаем путь к базе данных логов из конфигурации Serilog
        var logDbPath = configuration.GetValue<string>("Serilog:WriteTo:1:Args:sqliteDbPath") 
            ?? "data/logs.db";
        
        // Убедимся, что директория существует перед инициализацией Serilog
        var directory = Path.GetDirectoryName(logDbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Создаем логгер с настройками из конфигурации
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        return services;
    }
}
