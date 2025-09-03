// Название задачи: Консольное приложение для создания таблиц, включения CDC и генерации данных
// Описание задачи: Приложение создает таблицы в SQL Server, включает CDC и генерирует фейковые данные с помощью Bogus
// Чек-лист выполнения задачи:
// - [x] Создание таблиц
// - [x] Включение CDC
// - [x] Генерация фейковых данных
// - [x] Вставка данных в таблицы

using CdcGenerator.Configuration;
using CdcGenerator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CdcGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("CDC Generator - Пример работы с CDC в SQL Server");
        
        try
        {
            // Настройка конфигурации и сервисов
            var serviceProvider = ConfigureServices();
            
            // Получение сервисов
            var databaseService = serviceProvider.GetRequiredService<DatabaseService>();
            var dataGenerationService = serviceProvider.GetRequiredService<DataGenerationService>();
            
            // Создание базы данных и таблицы
            await databaseService.CreateDatabaseAndTableAsync();
            
            // Включение CDC для базы данных и таблицы
            await databaseService.EnableCdcAsync();
            
            // Генерация и вставка данных
            await dataGenerationService.GenerateAndInsertDataAsync();
            
            // ExampleApp не отправляет CDC-события напрямую
            // Эту функциональность выполняет Core сервис через плагины
            
            Console.WriteLine("Приложение успешно завершило работу. Нажмите любую клавишу для выхода.");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ReadKey();
        }
    }
    
    /// <summary>
    /// Configures application services
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        // Создание конфигурации из appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        // Создание настроек приложения из конфигурации
        var appSettings = new AppSettings();
        configuration.Bind(appSettings);
        
        // Настройка сервисов
        var services = new ServiceCollection();
        
        // Регистрация настроек как синглтон
        services.AddSingleton(appSettings);
        
        // Регистрация сервисов
        services.AddTransient<DatabaseService>();
        services.AddTransient<DataGenerationService>();
        
        return services.BuildServiceProvider();
    }
}