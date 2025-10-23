# Global Exception Handler Middleware

## Обзор

`GlobalExceptionHandlerMiddleware` обеспечивает централизованную обработку всех необработанных исключений в приложении CDC Bridge. Это устраняет необходимость в try-catch блоках в каждом контроллере и обеспечивает единообразный формат ответов об ошибках.

## Преимущества

### ? Было (try-catch в каждом контроллере)
```csharp
[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    try
    {
      var metrics = await _metricsService.GetMetricsAsync();
        return Ok(metrics);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving metrics");
        return StatusCode(500, "Internal server error");
    }
}
```

**Проблемы:**
- ? Дублирование кода обработки ошибок
- ? Необходимость внедрять ILogger в каждый контроллер
- ? Непостоянный формат ответов об ошибках
- ? Сложность поддержки и изменения логики обработки
- ? Ручное логирование в каждом месте

### ? Стало (Global Exception Handler)
```csharp
[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    var metrics = await _metricsService.GetMetricsAsync();
    return Ok(metrics);
}
```

**Преимущества:**
- ? Чистый и читаемый код контроллеров
- ? Автоматическое логирование всех ошибок
- ? Единообразный формат ответов
- ? Централизованная логика обработки
- ? Легко добавить новые типы исключений

## Архитектура

### Расположение в Pipeline

```
HTTP Request
     ?
GlobalExceptionHandlerMiddleware ? Перехватывает все исключения
   ?
ApiKeyAuthenticationMiddleware
     ?
Routing
     ?
Controllers (без try-catch)
     ?
Services (могут бросать исключения)
```

Middleware зарегистрирован **первым** в pipeline:

```csharp
// Program.cs
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// ... остальные middleware
```

## Формат ответа об ошибке

```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "details": "System.InvalidOperationException: ... (только в Development)",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/metrics"
}
```

### Поля ответа

| Поле | Тип | Описание |
|------|-----|----------|
| `statusCode` | int | HTTP статус код |
| `message` | string | Понятное сообщение об ошибке |
| `details` | string? | Stack trace (только в Development) |
| `timestamp` | DateTime | Время возникновения ошибки (UTC) |
| `path` | string | Путь запроса, вызвавшего ошибку |

## Маппинг исключений на HTTP статусы

| Тип исключения | HTTP статус | Сообщение |
|----------------|-------------|-----------|
| `ArgumentNullException` | 400 Bad Request | "Required parameter is missing" |
| `ArgumentException` | 400 Bad Request | Сообщение из исключения |
| `KeyNotFoundException` | 404 Not Found | "Resource not found" |
| `UnauthorizedAccessException` | 401 Unauthorized | "Unauthorized access" |
| `InvalidOperationException` | 400 Bad Request | Сообщение из исключения |
| `NotImplementedException` | 501 Not Implemented | "Feature not implemented" |
| **Все остальные** | 500 Internal Server Error | "An error occurred while processing your request" |

## Примеры использования

### Пример 1: Service бросает KeyNotFoundException

**Код сервиса:**
```csharp
public async Task<EventDto> GetEventByIdAsync(Guid id)
{
    var event = await _context.Events.FindAsync(id);
    if (event == null)
    {
        throw new KeyNotFoundException($"Event with id {id} not found");
    }
    return MapToDto(event);
}
```

**Контроллер (чистый код):**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<EventDto>> GetEventById(Guid id)
{
    var eventDto = await _eventsService.GetEventByIdAsync(id);
    return Ok(eventDto);
}
```

**Ответ клиенту (автоматически):**
```http
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "statusCode": 404,
  "message": "Resource not found",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/events/123e4567-e89b-12d3-a456-426614174000"
}
```

### Пример 2: Валидация параметров

**Код сервиса:**
```csharp
public async Task<MetricsDto> GetMetricsAsync()
{
    if (_configContext == null)
    {
      throw new InvalidOperationException("Configuration context is not initialized");
    }
    // ...
}
```

**Ответ клиенту:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "statusCode": 400,
  "message": "Configuration context is not initialized",
  "timestamp": "2024-01-15T10:30:00Z",
"path": "/api/metrics"
}
```

### Пример 3: Неожиданная ошибка (Production)

**Ответ в Production:**
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/configuration"
}
```

**Ответ в Development (с деталями):**
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "details": "System.NullReferenceException: Object reference not set to an instance of an object.\n   at CdcBridge.Host.Api.Services.MetricsService.GetMetricsAsync()...",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/metrics"
}
```

## Логирование

Все исключения автоматически логируются middleware:

```
[Error] Unhandled exception occurred. Request: GET /api/metrics
System.InvalidOperationException: Configuration context is not initialized
   at CdcBridge.Host.Api.Services.MetricsService.GetMetricsAsync()
   ...
```

**Формат лога:**
- Уровень: `Error`
- Сообщение: `"Unhandled exception occurred. Request: {Method} {Path}"`
- Исключение: Полный stack trace

## Расширение функциональности

### Добавление нового типа исключения

```csharp
private async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";

    var (statusCode, message) = exception switch
    {
        ArgumentNullException => (HttpStatusCode.BadRequest, "Required parameter is missing"),
 // ... существующие маппинги ...
        
        // Новый тип исключения
        MyCustomException customEx => (HttpStatusCode.UnprocessableEntity, customEx.Message),
        
        _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
    };

    // ... остальной код
}
```

### Добавление кастомных полей в ответ

```csharp
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public required string Message { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Path { get; set; }
    
    // Новые поля
    public string? TraceId { get; set; }
    public Dictionary<string, string>? ValidationErrors { get; set; }
}
```

### Интеграция с внешними системами мониторинга

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
  await _next(context);
  }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception occurred...");
        
        // Отправка в Sentry, Application Insights и т.д.
 await _errorTracker.TrackExceptionAsync(ex, context);
        
      await HandleExceptionAsync(context, ex);
    }
}
```

## Best Practices

### ? Рекомендуется

1. **Использовать специфичные исключения**
   ```csharp
throw new KeyNotFoundException($"Receiver '{name}' not found");
   // Вместо
   throw new Exception("Not found");
```

2. **Добавлять контекст в сообщения**
   ```csharp
   throw new InvalidOperationException($"Cannot process event {eventId}: tracking instance is disabled");
   ```

3. **Валидировать на уровне сервисов**
```csharp
   public async Task<Result> ProcessEvent(Event e)
   {
 if (e == null) throw new ArgumentNullException(nameof(e));
       if (!e.IsValid) throw new ArgumentException("Invalid event data", nameof(e));
     // ...
   }
   ```

### ? Не рекомендуется

1. **Скрывать ошибки пустыми catch блоками**
   ```csharp
   try
   {
       await service.DoSomething();
   }
   catch { } // ? Ошибка будет потеряна
   ```

2. **Возвращать HTTP статусы вручную при ошибках**
   ```csharp
   if (result == null)
   {
  return StatusCode(500, "Error"); // ? Используйте исключения
   }
   ```

3. **Логировать повторно в контроллерах**
   ```csharp
   try
   {
       var result = await service.GetData();
   return Ok(result);
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error"); // ? Middleware уже логирует
    throw;
   }
   ```

## Тестирование

### Unit тест для middleware

```csharp
[TestMethod]
public async Task InvokeAsync_ShouldReturn500_WhenUnhandledExceptionOccurs()
{
    // Arrange
    var context = new DefaultHttpContext();
    var logger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
    var env = new Mock<IHostEnvironment>();
    env.Setup(e => e.EnvironmentName).Returns("Production");
    
    RequestDelegate next = (HttpContext hc) => throw new Exception("Test exception");
    var middleware = new GlobalExceptionHandlerMiddleware(next, logger.Object, env.Object);
    
    context.Response.Body = new MemoryStream();
    
    // Act
    await middleware.InvokeAsync(context);
    
// Assert
    Assert.AreEqual(500, context.Response.StatusCode);
    Assert.AreEqual("application/json", context.Response.ContentType);
 
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
    Assert.IsTrue(response.Contains("An error occurred"));
}
```

### Integration тест

```csharp
[TestMethod]
public async Task GetMetrics_ShouldReturn500_WhenServiceThrowsException()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Симулируем ошибку в сервисе через моки
    _mockMetricsService.Setup(s => s.GetMetricsAsync())
     .ThrowsAsync(new InvalidOperationException("Service error"));
  
    // Act
    var response = await client.GetAsync("/api/metrics");
    
    // Assert
    Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    
    var content = await response.Content.ReadAsStringAsync();
    var error = JsonSerializer.Deserialize<ErrorResponse>(content);
    
    Assert.IsNotNull(error);
    Assert.AreEqual(500, error.StatusCode);
    Assert.IsTrue(error.Message.Contains("error occurred"));
}
```

## Миграция существующего кода

### Шаг 1: Удалите try-catch из контроллеров

**Было:**
```csharp
[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    try
    {
        var metrics = await _metricsService.GetMetricsAsync();
     return Ok(metrics);
    }
    catch (Exception ex)
  {
      _logger.LogError(ex, "Error retrieving metrics");
    return StatusCode(500, "Internal server error");
    }
}
```

**Стало:**
```csharp
[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    var metrics = await _metricsService.GetMetricsAsync();
    return Ok(metrics);
}
```

### Шаг 2: Удалите ILogger из контроллеров

Если logger использовался только для логирования ошибок, его можно полностью удалить из конструктора контроллера.

### Шаг 3: Улучшите исключения в сервисах

Вместо возврата null или пустых результатов, бросайте специфичные исключения:

```csharp
// Было
public async Task<EventDto?> GetEventByIdAsync(Guid id)
{
    var evt = await _context.Events.FindAsync(id);
    return evt != null ? MapToDto(evt) : null;
}

// Стало
public async Task<EventDto> GetEventByIdAsync(Guid id)
{
    var evt = await _context.Events.FindAsync(id);
    if (evt == null)
    {
        throw new KeyNotFoundException($"Event with id {id} not found");
    }
    return MapToDto(evt);
}
```

## Совместимость

- ? .NET 8.0+
- ? Работает с любыми middleware
- ? Не конфликтует с ApiKeyAuthenticationMiddleware
- ? Совместим с Swagger/OpenAPI
- ? Поддерживает async/await

## См. также

- [Middleware в ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Exception Handling в ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/error-handling)
- [Best Practices для Error Handling](https://docs.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
