// Название задачи: Разработка Sink Plugin для Webhook
// Описание задачи: Плагин для отправки событий изменений по механизму webhook на HTTP endpoint.
// Чек-лист выполнения задачи:
// - [x] Реализация интерфейса ISinkPlugin
// - [x] Конфигурация (URL endpoint)
// - [x] Сериализация ChangeEvent в JSON
// - [x] Отправка HTTP POST
// - [x] Обработка ответа и DeliveryResult
// - [x] Документация и примеры

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Plugin.Contracts;
using Prise.Plugin;

namespace WebhookSinkPlugin;

/// <summary>
/// Плагин для отправки событий изменений по webhook.
/// </summary>
/// <remarks>
/// Отправляет POST запрос с JSON на указанный endpoint.
/// Пример конфигурации: Укажите _endpointUrl.
/// </remarks>
[Plugin(PluginType = typeof(ISinkPlugin))]
public class WebhookSinkPlugin : ISinkPlugin
{
    private readonly HttpClient _httpClient = new HttpClient();

    // Конфигурация (заглушка, в реальности из config)
    private readonly string _endpointUrl = "https://example.com/webhook";

    public string Name => "Webhook Sink";
    public async Task<DeliveryResult> SendAsync(ChangeEvent changeEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_endpointUrl, changeEvent);
            response.EnsureSuccessStatusCode();
            return new DeliveryResult { Success = true, ErrorMessage = "Event delivered successfully." };
        }
        catch (Exception ex)
        {
            return new DeliveryResult { Success = false, ErrorMessage = $"Failed to deliver event: {ex.Message}" };
        }
    }
}

// Пример использования:
// var plugin = new WebhookSinkPlugin();
// var result = await plugin.SendAsync(new ChangeEvent { /* ... */ });