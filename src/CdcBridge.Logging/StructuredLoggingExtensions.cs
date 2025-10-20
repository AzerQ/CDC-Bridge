using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace CdcBridge.Logging;

/// <summary>
/// Методы расширения для настройки структурированного логирования в SQLite.
/// </summary>
public static class StructuredLoggingExtensions
{
    /// <summary>
    /// Добавляет структурированное логирование с хранением в SQLite.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var logDbPath = configuration.GetValue<string>("Logging:SqliteDbPath") ?? "data/logs.db";
        
        // Убедимся, что директория существует
        var directory = Path.GetDirectoryName(logDbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.SQLite(
                sqliteDbPath: logDbPath,
                tableName: "Logs",
                restrictedToMinimumLevel: LogEventLevel.Information,
                storeTimestampInUtc: true)
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        return services;
    }
}
