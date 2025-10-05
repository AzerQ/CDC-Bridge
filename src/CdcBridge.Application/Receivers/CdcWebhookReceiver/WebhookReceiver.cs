using System.Text;
using System.Text.Json;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;

namespace CdcBridge.Application.Receivers.CdcWebhookReceiver;

/// <summary>
/// Получатель, отправляющий события изменений на указанный webhook URL через HTTP.
/// Поддерживает настройку HTTP метода, заголовков и шаблона тела запроса.
/// </summary>
/// <remarks>
/// <para>
/// WebhookReceiver отправляет преобразованные данные событий на внешний HTTP endpoint.
/// Вся логика повторных попыток и обработки ошибок делегирована внешним системам.
/// </para>
/// <para>
/// Параметры конфигурации:
/// <list type="bullet">
/// <item>
/// <description><c>webhookUrl</c> - обязательный URL webhook</description>
/// </item>
/// <item>
/// <description><c>httpMethod</c> - HTTP метод (POST, PUT, PATCH, GET, DELETE), по умолчанию POST</description>
/// </item>
/// <item>
/// <description><c>headers</c> - опциональные HTTP заголовки</description>
/// </item>
/// <item>
/// <description><c>timeoutMs</c> - таймаут запроса в миллисекундах</description>
/// </item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Пример конфигурации YAML:
/// <code>
/// receivers:
///   - name: "AnalyticsWebhook"
///     type: "WebhookReceiver"
///     parameters:
///       webhookUrl: "https://api.analytics.com/webhooks/changes"
///       httpMethod: "POST"
///       timeoutMs: 30000
///       headers:
///         Authorization: "Bearer your-token-here"
///         Content-Type: "application/json"
///         User-Agent: "CDC-Bridge/1.0"
/// </code>
/// </example>
public class WebhookReceiver : IReceiver, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private IUrlTemplateRenderer<JsonElement> _urlTemplateRenderer;
    private bool _disposed = false;

    /// <summary>
    /// Инициализирует новый экземпляр Webhook получателя.
    /// </summary>
    /// Экземпляр HttpClient для отправки запросов. Если не указан, создается новый.
    /// </param>
    public WebhookReceiver()
    {
        _httpClient = MakeHttpClient();
        _urlTemplateRenderer = new HandlebarsUrlTemplateRenderer();
    }

    private HttpClient MakeHttpClient()
    {
        HttpClientHandler handler = new HttpClientHandler();
        handler.UseDefaultCredentials = true; // This enables the use of default network credentials
        return new HttpClient(handler);
    }

    /// <summary>
    /// Уникальное имя получателя.
    /// </summary>
    public string Name => nameof(WebhookReceiver);

    /// <summary>
    /// Асинхронно отправляет событие изменения на настроенный webhook URL.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для отправки.</param>
    /// <param name="parameters">
    /// Параметры webhook в формате JSON. Должен содержать свойство "webhookUrl".
    /// </param>
    /// <returns>
    /// Результат обработки, содержащий статус отправки и описание ошибки при необходимости.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// trackedChange или parameters являются null.
    /// </exception>
    public async Task<ReceiverProcessResult> SendAsync(TrackedChange trackedChange, JsonElement parameters)
    {
        if (trackedChange == null)
            throw new ArgumentNullException(nameof(trackedChange));
        
        if (parameters.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            throw new ArgumentNullException(nameof(parameters));

        try
        {
            var config = ParseWebhookConfig(parameters);
            return await SendWebhookAsync(trackedChange, config);
        }
        catch (Exception ex)
        {
            return new ReceiverProcessResult
            {
                Status = ReceiverProcessStatus.Failure,
                ErrorDescription = $"Failed to send webhook at: {ex.Message}"
            };
        }
    }

    private async Task<ReceiverProcessResult> SendWebhookAsync(TrackedChange trackedChange, WebhookConfig config)
    {
        using var request = CreateHttpRequest(trackedChange, config);
        using var timeoutCts = new CancellationTokenSource(config.TimeoutMs);

        try
        {
            var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            
            if (response.IsSuccessStatusCode)
            {
                return new ReceiverProcessResult
                {
                    Status = ReceiverProcessStatus.Success
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ReceiverProcessResult
            {
                Status = ReceiverProcessStatus.Failure,
                ErrorDescription = $"HTTP {(int)response.StatusCode} {response.StatusCode}. Response: {errorContent}"
            };
        }
        catch (TaskCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            return new ReceiverProcessResult
            {
                Status = ReceiverProcessStatus.Failure,
                ErrorDescription = $"Webhook request timed out after {config.TimeoutMs}ms"
            };
        }
        catch (HttpRequestException ex)
        {
            return new ReceiverProcessResult
            {
                Status = ReceiverProcessStatus.Failure,
                ErrorDescription = $"HTTP request failed: {ex.Message}"
            };
        }
    }

    private HttpRequestMessage CreateHttpRequest(TrackedChange trackedChange, WebhookConfig config)
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(ResolveRequestUrl(trackedChange, config)),
            Method = GetHttpMethod(config.HttpMethod)
        };

        // Добавляем заголовки
        foreach (var header in config.Headers ?? new Dictionary<string, string>())
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Добавляем заголовок User-Agent по умолчанию, если не указан
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add("User-Agent", "CDC-Bridge-Webhook/1.0");
        }

        if (request.Method == HttpMethod.Get ||
            request.Method == HttpMethod.Delete) return request;
        
        // Сериализуем тело запроса
        var requestBody = SerializeRequestBody(trackedChange, config);
        request.Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");

        return request;
    }

    string ResolveRequestUrl(TrackedChange trackedChange, WebhookConfig config)
    {
        if (!config.UrlIsTemplate.HasValue || !config.UrlIsTemplate.Value)
            return config.WebhookUrl;

        return _urlTemplateRenderer.RenderUrlTemplate(config.WebhookUrl, SerializeTrackedChange(trackedChange));
    }

    private static JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    private JsonElement SerializeTrackedChange(TrackedChange trackedChange) => JsonSerializer.SerializeToElement(trackedChange, _jsonSerializerOptions);

    private JsonElement SerializeRequestBody(TrackedChange trackedChange, WebhookConfig config) => 
        trackedChange.Data.TransformedData != null ? JsonSerializer.SerializeToElement(trackedChange.Data.TransformedData, _jsonSerializerOptions) : SerializeTrackedChange(trackedChange);

    private WebhookConfig ParseWebhookConfig(JsonElement parameters)
    {
        try
        {
            var config = parameters.Deserialize<WebhookConfig>(JsonSerializerOptions.Web) 
                ?? throw new InvalidOperationException("Failed to deserialize webhook configuration.");

            ValidateWebhookConfig(config);
            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid webhook configuration in receiver '{Name}'.", ex);
        }
    }

    private static HttpMethod GetHttpMethod(string httpMethod)
    {
        return httpMethod?.ToUpperInvariant() switch
        {
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "GET" => HttpMethod.Get,
            "DELETE" => HttpMethod.Delete,
            _ => HttpMethod.Post
        };
    }

    private static void ValidateWebhookConfig(WebhookConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.WebhookUrl))
            throw new InvalidOperationException("Webhook URL cannot be null or empty.");

        if (!Uri.TryCreate(config.WebhookUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"Invalid webhook URL: {config.WebhookUrl}");

        if (config.TimeoutMs <= 0)
            throw new InvalidOperationException("Timeout must be greater than 0.");
    }

    /// <summary>
    /// Освобождает ресурсы HttpClient.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
        
        await ValueTask.CompletedTask;
    }
}