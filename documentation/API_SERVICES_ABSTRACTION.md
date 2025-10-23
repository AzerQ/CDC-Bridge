# Внедрение абстракций для API сервисов

## Обзор

Все API сервисы в проекте `CdcBridge.Host` теперь используют правильную архитектуру с интерфейсами и внедрением зависимостей (DI).

## Созданные интерфейсы

### 1. ILogsService
```csharp
public interface ILogsService
{
    Task<PagedResultDto<LogEntryDto>> GetLogsAsync(LogQueryDto query);
}
```

**Назначение:** Работа с логами, хранящимися в SQLite (Serilog).

**Реализация:** `LogsService`

### 2. IMetricsService
```csharp
public interface IMetricsService
{
    Task<MetricsDto> GetMetricsAsync();
}
```

**Назначение:** Получение метрик системы CDC Bridge.

**Реализация:** `MetricsService`

### 3. IEventsService
```csharp
public interface IEventsService
{
    Task<PagedResultDto<EventDto>> GetEventsAsync(EventQueryDto query);
    Task<EventDto?> GetEventByIdAsync(Guid id);
}
```

**Назначение:** Работа с событиями изменений данных.

**Реализация:** `EventsService`

## Регистрация в DI-контейнере

Все сервисы зарегистрированы в `Program.cs` через интерфейсы:

```csharp
// Add API services
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<ILogsService, LogsService>();
```

## Использование в контроллерах

Контроллеры теперь зависят от интерфейсов, а не конкретных реализаций:

### LogsController
```csharp
public class LogsController : ControllerBase
{
    private readonly ILogsService _logsService;

    public LogsController(ILogsService logsService)
    {
_logsService = logsService;
    }
    
    // ...методы контроллера
}
```

### MetricsController
```csharp
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;

    public MetricsController(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }
    
    // ...методы контроллера
}
```

### EventsController
```csharp
public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;

    public EventsController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }
    
// ...методы контроллера
}
```

## Преимущества

### 1. **Тестируемость**
- Легко создавать mock-объекты для unit-тестов
- Можно тестировать контроллеры изолированно от реальных сервисов

```csharp
// Пример unit-теста
var mockLogsService = new Mock<ILogsService>();
var controller = new LogsController(mockLogsService.Object);
```

### 2. **Гибкость**
- Можно легко заменить реализацию без изменения контроллеров
- Возможность создания альтернативных реализаций (например, для кэширования)

### 3. **Соблюдение SOLID принципов**
- **Dependency Inversion Principle**: Высокоуровневые модули (контроллеры) зависят от абстракций, а не от конкретных реализаций
- **Single Responsibility Principle**: Каждый сервис имеет единственную ответственность
- **Interface Segregation Principle**: Интерфейсы минималистичны и специфичны

### 4. **Упрощение DI**
- ASP.NET Core автоматически разрешает зависимости через интерфейсы
- Явная видимость зависимостей в конструкторах

## Структура файлов

```
src/CdcBridge.Host/
??? Api/
    ??? Controllers/
    ?   ??? LogsController.cs
    ?   ??? MetricsController.cs
    ?   ??? EventsController.cs
    ??? Services/
        ??? ILogsService.cs          ? Новый интерфейс
     ??? LogsService.cs           ? Реализация
        ??? IMetricsService.cs    ? Новый интерфейс
        ??? MetricsService.cs        ? Реализация
   ??? IEventsService.cs        ? Новый интерфейс
        ??? EventsService.cs         ? Реализация
```

## Время жизни сервисов

Все API сервисы зарегистрированы с временем жизни **Scoped**:

```csharp
builder.Services.AddScoped<ILogsService, LogsService>();
```

**Scoped** означает, что:
- Один экземпляр сервиса создается на каждый HTTP-запрос
- Экземпляр удаляется после завершения запроса
- Подходит для сервисов, работающих с базой данных в рамках одного запроса

## Примеры расширения

### Добавление кэширования
```csharp
public class CachedLogsService : ILogsService
{
    private readonly ILogsService _innerService;
    private readonly IMemoryCache _cache;

    public CachedLogsService(LogsService innerService, IMemoryCache cache)
    {
        _innerService = innerService;
   _cache = cache;
    }

    public async Task<PagedResultDto<LogEntryDto>> GetLogsAsync(LogQueryDto query)
    {
        var cacheKey = $"logs_{query.GetHashCode()}";
      return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
       return await _innerService.GetLogsAsync(query);
        });
    }
}
```

### Добавление логирования (Decorator pattern)
```csharp
public class LoggedMetricsService : IMetricsService
{
    private readonly IMetricsService _innerService;
    private readonly ILogger<LoggedMetricsService> _logger;

    public LoggedMetricsService(MetricsService innerService, ILogger<LoggedMetricsService> logger)
    {
    _innerService = innerService;
        _logger = logger;
    }

    public async Task<MetricsDto> GetMetricsAsync()
    {
        _logger.LogInformation("Requesting system metrics...");
  var metrics = await _innerService.GetMetricsAsync();
        _logger.LogInformation("Metrics retrieved successfully");
     return metrics;
    }
}
```

## Совместимость

Все изменения полностью обратно совместимы. API endpoints остались без изменений:

- `GET /api/Logs` - работает как прежде
- `GET /api/Metrics` - работает как прежде
- `GET /api/Events` - работает как прежде
- `GET /api/Events/{id}` - работает как прежде

## Заключение

Внедрение абстракций для API сервисов улучшает архитектуру проекта, делая его:
- ? Более тестируемым
- ? Более гибким для изменений
- ? Соответствующим лучшим практикам .NET
- ? Готовым к расширению функциональности
