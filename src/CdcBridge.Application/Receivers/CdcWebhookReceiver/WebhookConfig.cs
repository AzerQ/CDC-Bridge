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
    public required string WebhookUrl { get; set; }

    /// <summary>
    /// HTTP метод для запроса.
    /// </summary>
    public required string HttpMethod { get; set; }

    /// <summary>
    /// Таймаут запроса в миллисекундах.
    /// </summary>
    public required int TimeoutMs { get; set; }

    /// <summary>
    /// HTTP заголовки для запроса.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}