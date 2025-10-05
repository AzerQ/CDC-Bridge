namespace CdcBridge.Application.Receivers.CdcWebhookReceiver;

/// <summary>
/// Конфигурация webhook получателя.
/// </summary>
public class WebhookConfig
{
    
    /// <summary>
    /// Флаг указывающий на то что URL это Handlebars шаблон в который будут подставлены данные из модели изменения
    /// </summary>
    public bool? UrlIsTemplate { get; set; }
    
    /// <summary>
    /// URL webhook для отправки событий.
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// HTTP метод для запроса. По умолчанию: "POST".
    /// </summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// Таймаут запроса в миллисекундах. По умолчанию: 30000 (30 секунд).
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// HTTP заголовки для запроса.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}