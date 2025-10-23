using CdcBridge.Core.Models;
using Microsoft.EntityFrameworkCore;
using CdcBridge.Persistence;

namespace CdcBridge.Host.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
 _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IDbContextFactory<CdcBridgeDbContext> contextFactory)
    {
    // Пропускаем Swagger, health check и localhost-only endpoints
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health") ||
   context.Request.Path.StartsWithSegments("/api/admin"))
   {
       await _next(context);
        return;
      }

   if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
      {
 _logger.LogWarning("API request without API Key from {RemoteIp}", context.Connection.RemoteIpAddress);
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "API Key is missing. Please provide X-API-Key header." });
   return;
        }

        await using var dbContext = await contextFactory.CreateDbContextAsync();
    var apiKey = await dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == extractedApiKey.First() && k.IsActive);

        if (apiKey == null)
        {
            _logger.LogWarning("Invalid API Key attempt from {RemoteIp}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = 401;
 await context.Response.WriteAsJsonAsync(new { error = "Invalid or inactive API Key" });
            return;
        }

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
        {
     _logger.LogWarning("Expired API Key used: {ApiKeyName} from {RemoteIp}", apiKey.Name, context.Connection.RemoteIpAddress);
     context.Response.StatusCode = 401;
      await context.Response.WriteAsJsonAsync(new { error = "API Key has expired" });
    return;
        }

        // Проверка прав доступа для методов изменения данных
    var writeMethods = new[] { "POST", "PUT", "PATCH", "DELETE" };
        if (writeMethods.Contains(context.Request.Method) && apiKey.Permission == ApiKeyPermission.ReadOnly)
        {
 _logger.LogWarning("Write operation attempted with ReadOnly API Key: {ApiKeyName} from {RemoteIp}", 
  apiKey.Name, context.Connection.RemoteIpAddress);
     context.Response.StatusCode = 403;
     await context.Response.WriteAsJsonAsync(new { error = "This API Key has read-only permissions" });
            return;
        }

        // Обновляем время последнего использования (делаем это асинхронно без ожидания)
_ = Task.Run(async () =>
   {
      try
    {
          await using var updateContext = await contextFactory.CreateDbContextAsync();
       var keyToUpdate = await updateContext.ApiKeys.FindAsync(apiKey.Id);
  if (keyToUpdate != null)
      {
   keyToUpdate.LastUsedAt = DateTime.UtcNow;
          await updateContext.SaveChangesAsync();
   }
       }
        catch (Exception ex)
         {
  _logger.LogError(ex, "Failed to update LastUsedAt for API Key {ApiKeyId}", apiKey.Id);
            }
        });

        // Добавляем информацию о ключе в контекст для использования в контроллерах
        context.Items["ApiKey"] = apiKey;
        context.Items["ApiKeyName"] = apiKey.Name;
     context.Items["ApiKeyPermission"] = apiKey.Permission;

        _logger.LogDebug("API Key authenticated: {ApiKeyName} ({Permission})", apiKey.Name, apiKey.Permission);

     await _next(context);
    }
}
