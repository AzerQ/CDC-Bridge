# Global Exception Handler Middleware

## �����

`GlobalExceptionHandlerMiddleware` ������������ ���������������� ��������� ���� �������������� ���������� � ���������� CDC Bridge. ��� ��������� ������������� � try-catch ������ � ������ ����������� � ������������ ������������� ������ ������� �� �������.

## ������������

### ? ���� (try-catch � ������ �����������)
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

**��������:**
- ? ������������ ���� ��������� ������
- ? ������������� �������� ILogger � ������ ����������
- ? ������������ ������ ������� �� �������
- ? ��������� ��������� � ��������� ������ ���������
- ? ������ ����������� � ������ �����

### ? ����� (Global Exception Handler)
```csharp
[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    var metrics = await _metricsService.GetMetricsAsync();
    return Ok(metrics);
}
```

**������������:**
- ? ������ � �������� ��� ������������
- ? �������������� ����������� ���� ������
- ? ������������� ������ �������
- ? ���������������� ������ ���������
- ? ����� �������� ����� ���� ����������

## �����������

### ������������ � Pipeline

```
HTTP Request
     ?
GlobalExceptionHandlerMiddleware ? ������������� ��� ����������
   ?
ApiKeyAuthenticationMiddleware
     ?
Routing
     ?
Controllers (��� try-catch)
     ?
Services (����� ������� ����������)
```

Middleware ��������������� **������** � pipeline:

```csharp
// Program.cs
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// ... ��������� middleware
```

## ������ ������ �� ������

```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "details": "System.InvalidOperationException: ... (������ � Development)",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/metrics"
}
```

### ���� ������

| ���� | ��� | �������� |
|------|-----|----------|
| `statusCode` | int | HTTP ������ ��� |
| `message` | string | �������� ��������� �� ������ |
| `details` | string? | Stack trace (������ � Development) |
| `timestamp` | DateTime | ����� ������������� ������ (UTC) |
| `path` | string | ���� �������, ���������� ������ |

## ������� ���������� �� HTTP �������

| ��� ���������� | HTTP ������ | ��������� |
|----------------|-------------|-----------|
| `ArgumentNullException` | 400 Bad Request | "Required parameter is missing" |
| `ArgumentException` | 400 Bad Request | ��������� �� ���������� |
| `KeyNotFoundException` | 404 Not Found | "Resource not found" |
| `UnauthorizedAccessException` | 401 Unauthorized | "Unauthorized access" |
| `InvalidOperationException` | 400 Bad Request | ��������� �� ���������� |
| `NotImplementedException` | 501 Not Implemented | "Feature not implemented" |
| **��� ���������** | 500 Internal Server Error | "An error occurred while processing your request" |

## ������� �������������

### ������ 1: Service ������� KeyNotFoundException

**��� �������:**
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

**���������� (������ ���):**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<EventDto>> GetEventById(Guid id)
{
    var eventDto = await _eventsService.GetEventByIdAsync(id);
    return Ok(eventDto);
}
```

**����� ������� (�������������):**
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

### ������ 2: ��������� ����������

**��� �������:**
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

**����� �������:**
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

### ������ 3: ����������� ������ (Production)

**����� � Production:**
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/configuration"
}
```

**����� � Development (� ��������):**
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "details": "System.NullReferenceException: Object reference not set to an instance of an object.\n   at CdcBridge.Host.Api.Services.MetricsService.GetMetricsAsync()...",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/metrics"
}
```

## �����������

��� ���������� ������������� ���������� middleware:

```
[Error] Unhandled exception occurred. Request: GET /api/metrics
System.InvalidOperationException: Configuration context is not initialized
   at CdcBridge.Host.Api.Services.MetricsService.GetMetricsAsync()
   ...
```

**������ ����:**
- �������: `Error`
- ���������: `"Unhandled exception occurred. Request: {Method} {Path}"`
- ����������: ������ stack trace

## ���������� ����������������

### ���������� ������ ���� ����������

```csharp
private async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";

    var (statusCode, message) = exception switch
    {
        ArgumentNullException => (HttpStatusCode.BadRequest, "Required parameter is missing"),
 // ... ������������ �������� ...
        
        // ����� ��� ����������
        MyCustomException customEx => (HttpStatusCode.UnprocessableEntity, customEx.Message),
        
        _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
    };

    // ... ��������� ���
}
```

### ���������� ��������� ����� � �����

```csharp
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public required string Message { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Path { get; set; }
    
    // ����� ����
    public string? TraceId { get; set; }
    public Dictionary<string, string>? ValidationErrors { get; set; }
}
```

### ���������� � �������� ��������� �����������

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
        
        // �������� � Sentry, Application Insights � �.�.
 await _errorTracker.TrackExceptionAsync(ex, context);
        
      await HandleExceptionAsync(context, ex);
    }
}
```

## Best Practices

### ? �������������

1. **������������ ����������� ����������**
   ```csharp
throw new KeyNotFoundException($"Receiver '{name}' not found");
   // ������
   throw new Exception("Not found");
```

2. **��������� �������� � ���������**
   ```csharp
   throw new InvalidOperationException($"Cannot process event {eventId}: tracking instance is disabled");
   ```

3. **������������ �� ������ ��������**
```csharp
   public async Task<Result> ProcessEvent(Event e)
   {
 if (e == null) throw new ArgumentNullException(nameof(e));
       if (!e.IsValid) throw new ArgumentException("Invalid event data", nameof(e));
     // ...
   }
   ```

### ? �� �������������

1. **�������� ������ ������� catch �������**
   ```csharp
   try
   {
       await service.DoSomething();
   }
   catch { } // ? ������ ����� ��������
   ```

2. **���������� HTTP ������� ������� ��� �������**
   ```csharp
   if (result == null)
   {
  return StatusCode(500, "Error"); // ? ����������� ����������
   }
   ```

3. **���������� �������� � ������������**
   ```csharp
   try
   {
       var result = await service.GetData();
   return Ok(result);
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error"); // ? Middleware ��� ��������
    throw;
   }
   ```

## ������������

### Unit ���� ��� middleware

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

### Integration ����

```csharp
[TestMethod]
public async Task GetMetrics_ShouldReturn500_WhenServiceThrowsException()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // ���������� ������ � ������� ����� ����
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

## �������� ������������� ����

### ��� 1: ������� try-catch �� ������������

**����:**
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

**�����:**
```csharp
[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    var metrics = await _metricsService.GetMetricsAsync();
    return Ok(metrics);
}
```

### ��� 2: ������� ILogger �� ������������

���� logger ������������� ������ ��� ����������� ������, ��� ����� ��������� ������� �� ������������ �����������.

### ��� 3: �������� ���������� � ��������

������ �������� null ��� ������ �����������, �������� ����������� ����������:

```csharp
// ����
public async Task<EventDto?> GetEventByIdAsync(Guid id)
{
    var evt = await _context.Events.FindAsync(id);
    return evt != null ? MapToDto(evt) : null;
}

// �����
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

## �������������

- ? .NET 8.0+
- ? �������� � ������ middleware
- ? �� ����������� � ApiKeyAuthenticationMiddleware
- ? ��������� � Swagger/OpenAPI
- ? ������������ async/await

## ��. �����

- [Middleware � ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Exception Handling � ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/error-handling)
- [Best Practices ��� Error Handling](https://docs.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
