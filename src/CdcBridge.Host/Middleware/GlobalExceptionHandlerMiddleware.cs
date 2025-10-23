using System.Net;
using System.Text.Json;

namespace CdcBridge.Host.Middleware;

/// <summary>
/// Middleware для централизованной обработки исключений в приложении.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
     IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Unhandled exception occurred. Request: {Method} {Path}",
                context.Request.Method,
      context.Request.Path);

      await HandleExceptionAsync(context, ex);
 }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Required parameter is missing"),
       ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
        KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
   InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
     NotImplementedException => (HttpStatusCode.NotImplemented, "Feature not implemented"),
            _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
        };

        context.Response.StatusCode = (int)statusCode;

    var response = new ErrorResponse
  {
        StatusCode = (int)statusCode,
      Message = message,
            Details = _environment.IsDevelopment() ? exception.ToString() : null,
       Timestamp = DateTime.UtcNow,
     Path = context.Request.Path
};

        var options = new JsonSerializerOptions
      {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Модель ответа об ошибке.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP статус код.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Сообщение об ошибке.
/// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Детальная информация об ошибке (только в Development режиме).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Временная метка возникновения ошибки.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Путь запроса, вызвавшего ошибку.
  /// </summary>
  public required string Path { get; set; }
}
