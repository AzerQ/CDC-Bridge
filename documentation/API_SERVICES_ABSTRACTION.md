# ��������� ���������� ��� API ��������

## �����

��� API ������� � ������� `CdcBridge.Host` ������ ���������� ���������� ����������� � ������������ � ���������� ������������ (DI).

## ��������� ����������

### 1. ILogsService
```csharp
public interface ILogsService
{
    Task<PagedResultDto<LogEntryDto>> GetLogsAsync(LogQueryDto query);
}
```

**����������:** ������ � ������, ����������� � SQLite (Serilog).

**����������:** `LogsService`

### 2. IMetricsService
```csharp
public interface IMetricsService
{
    Task<MetricsDto> GetMetricsAsync();
}
```

**����������:** ��������� ������ ������� CDC Bridge.

**����������:** `MetricsService`

### 3. IEventsService
```csharp
public interface IEventsService
{
    Task<PagedResultDto<EventDto>> GetEventsAsync(EventQueryDto query);
    Task<EventDto?> GetEventByIdAsync(Guid id);
}
```

**����������:** ������ � ��������� ��������� ������.

**����������:** `EventsService`

## ����������� � DI-����������

��� ������� ���������������� � `Program.cs` ����� ����������:

```csharp
// Add API services
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<ILogsService, LogsService>();
```

## ������������� � ������������

����������� ������ ������� �� �����������, � �� ���������� ����������:

### LogsController
```csharp
public class LogsController : ControllerBase
{
    private readonly ILogsService _logsService;

    public LogsController(ILogsService logsService)
    {
_logsService = logsService;
    }
    
    // ...������ �����������
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
    
    // ...������ �����������
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
    
// ...������ �����������
}
```

## ������������

### 1. **�������������**
- ����� ��������� mock-������� ��� unit-������
- ����� ����������� ����������� ������������ �� �������� ��������

```csharp
// ������ unit-�����
var mockLogsService = new Mock<ILogsService>();
var controller = new LogsController(mockLogsService.Object);
```

### 2. **��������**
- ����� ����� �������� ���������� ��� ��������� ������������
- ����������� �������� �������������� ���������� (��������, ��� �����������)

### 3. **���������� SOLID ���������**
- **Dependency Inversion Principle**: ��������������� ������ (�����������) ������� �� ����������, � �� �� ���������� ����������
- **Single Responsibility Principle**: ������ ������ ����� ������������ ���������������
- **Interface Segregation Principle**: ���������� �������������� � ����������

### 4. **��������� DI**
- ASP.NET Core ������������� ��������� ����������� ����� ����������
- ����� ��������� ������������ � �������������

## ��������� ������

```
src/CdcBridge.Host/
??? Api/
    ??? Controllers/
    ?   ??? LogsController.cs
    ?   ??? MetricsController.cs
    ?   ??? EventsController.cs
    ??? Services/
        ??? ILogsService.cs          ? ����� ���������
     ??? LogsService.cs           ? ����������
        ??? IMetricsService.cs    ? ����� ���������
        ??? MetricsService.cs        ? ����������
   ??? IEventsService.cs        ? ����� ���������
        ??? EventsService.cs         ? ����������
```

## ����� ����� ��������

��� API ������� ���������������� � �������� ����� **Scoped**:

```csharp
builder.Services.AddScoped<ILogsService, LogsService>();
```

**Scoped** ��������, ���:
- ���� ��������� ������� ��������� �� ������ HTTP-������
- ��������� ��������� ����� ���������� �������
- �������� ��� ��������, ���������� � ����� ������ � ������ ������ �������

## ������� ����������

### ���������� �����������
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

### ���������� ����������� (Decorator pattern)
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

## �������������

��� ��������� ��������� ������� ����������. API endpoints �������� ��� ���������:

- `GET /api/Logs` - �������� ��� ������
- `GET /api/Metrics` - �������� ��� ������
- `GET /api/Events` - �������� ��� ������
- `GET /api/Events/{id}` - �������� ��� ������

## ����������

��������� ���������� ��� API �������� �������� ����������� �������, ����� ���:
- ? ����� �����������
- ? ����� ������ ��� ���������
- ? ��������������� ������ ��������� .NET
- ? ������� � ���������� ����������������
