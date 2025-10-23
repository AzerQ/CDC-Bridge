# �����������: ���������������� ��������� ������

## ������ ���������

����������� ���������������� ��������� ���������� ����� `GlobalExceptionHandlerMiddleware`, ��� ��������� ������������� � try-catch ������ � ������ �����������.

## ? ��� ���� �������

### 1. ������ GlobalExceptionHandlerMiddleware

**����:** `src/CdcBridge.Host/Middleware/GlobalExceptionHandlerMiddleware.cs`

**����������������:**
- �������� ���� �������������� ����������
- �������������� ����������� ������
- ������� ���������� �� HTTP �������
- ������������� ������ JSON �������
- ������� stack trace � Production

**�������������� ����������:**
| ���������� | HTTP Status | ��������� |
|-----------|-------------|-----------|
| `ArgumentNullException` | 400 | "Required parameter is missing" |
| `ArgumentException` | 400 | ��������� �� ���������� |
| `KeyNotFoundException` | 404 | "Resource not found" |
| `UnauthorizedAccessException` | 401 | "Unauthorized access" |
| `InvalidOperationException` | 400 | ��������� �� ���������� |
| `NotImplementedException` | 501 | "Feature not implemented" |
| ��������� | 500 | "An error occurred..." |

### 2. �������� Program.cs

```csharp
// Middleware ��������������� ������ � pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

### 3. �������� ��� �����������

������� try-catch ����� � ILogger ��:
- ? `ConfigurationController.cs` (���� 3 ������ � try-catch ? ����� 3 ������ ������)
- ? `MetricsController.cs` (���� 1 ����� � try-catch ? ����� 1 ������ �����)
- ? `EventsController.cs` (���� 2 ������ � try-catch ? ����� 2 ������ ������)
- ? `LogsController.cs` (���� 1 ����� � try-catch ? ����� 1 ������ �����)

**������ ���������:**

**����:**
```csharp
private readonly MetricsService _metricsService;
private readonly ILogger<MetricsController> _logger;

public MetricsController(MetricsService metricsService, ILogger<MetricsController> logger)
{
_metricsService = metricsService;
    _logger = logger;
}

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
private readonly MetricsService _metricsService;

public MetricsController(MetricsService metricsService)
{
    _metricsService = metricsService;
}

[HttpGet]
public async Task<ActionResult<MetricsDto>> GetMetrics()
{
    var metrics = await _metricsService.GetMetricsAsync();
    return Ok(metrics);
}
```

### 4. ������� �������������� �����������

**����:** `src/CdcBridge.Host/CdcBridge.Host.csproj`

������ ����� `Microsoft.AspNetCore.Authentication.JwtBearer` (������ �� ������������ ����� �������� �� API �����).

### 5. ������� ������������

**����:** `GLOBAL_EXCEPTION_HANDLER.md`

������ ����������� �� ������������� middleware:
- ����������� � ������� ������
- ������� �������������
- Best practices
- ������������
- �������� ������������� ����

## ?? ������� ���������

### ��� ������������

| ������� | ���� | ����� | ��������� |
|---------|------|-------|-----------|
| ����� ���� � ������������ | ~180 | ~90 | -50% |
| Try-catch ������ | 7 | 0 | -100% |
| ILogger � ������������ | 4 | 0 | -100% |
| ������������ ������ | ������� | ��� | -100% |

### Maintainability

- ? **����������:** ��� ������������ ���� ����������� ����
- ? **���������:** ������ ��������� ������ � ����� �����
- ? **�������������:** ����������� ����� ����������� ��� mock ��������
- ? **�������������:** ����� �������� ����� ���� ����������

## ?? ������ ������ �� ������

```json
{
  "statusCode": 404,
  "message": "Resource not found",
  "details": "System.KeyNotFoundException: Event not found... (������ � Development)",
  "timestamp": "2024-01-15T10:30:00Z",
  "path": "/api/events/123"
}
```

## ? ��������

```sh
# ������ �������
dotnet build CDC-Bridge.sln
# Build succeeded. 0 Warning(s) 0 Error(s)
```

## ?? ����� ��������

1. ? `src/CdcBridge.Host/Middleware/GlobalExceptionHandlerMiddleware.cs` - **������**
2. ? `src/CdcBridge.Host/Program.cs` - �������� middleware
3. ? `src/CdcBridge.Host/CdcBridge.Host.csproj` - ������ JWT �����
4. ? `src/CdcBridge.Host/Api/Controllers/ConfigurationController.cs` - �������
5. ? `src/CdcBridge.Host/Api/Controllers/MetricsController.cs` - �������
6. ? `src/CdcBridge.Host/Api/Controllers/EventsController.cs` - �������
7. ? `src/CdcBridge.Host/Api/Controllers/LogsController.cs` - �������
8. ? `GLOBAL_EXCEPTION_HANDLER.md` - **������**

## ?? ��������� ����

������������ ��� ����������� ���������:

1. **�������� ����������� ���������� ��� ������**
   ```csharp
   public class TrackingInstanceNotFoundException : KeyNotFoundException
   {
       public TrackingInstanceNotFoundException(string name) 
           : base($"Tracking instance '{name}' not found") { }
   }
   ```

2. **�������� ��������� �� ������ ��������**
   ```csharp
   if (string.IsNullOrEmpty(receiverName))
       throw new ArgumentNullException(nameof(receiverName));
   ```

3. **������������� � �������� �����������**
   - Application Insights
   - Sentry
   - ELK Stack

4. **�������� Rate Limiting ��� ������ �� DoS**

## ?? Best Practices ���������

- ? Single Responsibility Principle - ������ ��������� ������ ����
- ? DRY (Don't Repeat Yourself) - ��� ������������
- ? Separation of Concerns - ��������� ������ ��������
- ? Open/Closed Principle - ����� ��������� ��� ���������
- ? Fail Fast - ������ �������������� �����

## ?? ���������

��� ����:
- **����** - ������ ���������� ����
- **����������** - ������������� ��������� ������
- **�������** - ����� ������������ � ���������
- **����������������** - ������������� best practices
