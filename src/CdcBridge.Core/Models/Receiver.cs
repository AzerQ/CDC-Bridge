using System;
using System.Text.Json.Nodes;

namespace CdcBridge.Core.Models;

/// <summary>
/// Получатель уведомлений о событиях (канал доставки).
/// Конфигурируется типом канала и набором параметров, зависящих от типа.
/// </summary>
public class Receiver
{
	/// <summary>
	/// Уникальное имя канала получателя.
	/// Используется для идентификации и ссылок в конфигурации.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Произвольное описание назначения канала.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Идентификатор отслеживаемого источника, для которого отправляются события.
	/// Рекомендуется указывать значение <c>TrackingInstance.sourceTable</c> из конфигурации.
	/// </summary>
	public required string TrackingInstance { get; set; }

	/// <summary>
	/// Необязательная ссылка на фильтр по имени (<c>Filter.Name</c>),
	/// позволяющий отобрать события перед отправкой.
	/// </summary>
	public string? Filter { get; set; }

	/// <summary>
	/// Необязательная ссылка на трансформер по имени (<c>Transformer.Name</c>),
	/// который будет применён к полезной нагрузке перед отправкой.
	/// </summary>
	public string? Transformer { get; set; }

	/// <summary>
	/// Тип канала доставки. Определяет используемый транспорт и схему параметров.
	/// Примеры: <c>WebhookReceiver</c>, <c>MyKafkaReceiver</c> и др.
	/// </summary>
	public required string Type { get; set; }

	/// <summary>
	/// Количество повторных попыток отправки при ошибке.
	/// Значение по умолчанию: 0 (без повторов).
	/// </summary>
	public int RetryCount { get; set; } = 0;

	/// <summary>
	/// Параметры канала доставки. Набор полей зависит от значения <see cref="Type"/>.
	/// </summary>
	/// <remarks>
	/// Примеры параметров по типам:
	/// <para>
	/// Type = "WebhookReceiver"
	/// Parameters = {
	///   "webhookUrl": "https://example.com/hook",
	///   "httpMethod": "POST",
	///   "headers": { "Authorization": "Bearer ...", "Content-Type": "application/json" },
	///   "bodyTemplate": "{ \"id\": \"{{data.new.id}}\" }"
	/// }
	/// </para>
	/// <para>
	/// Type = "MyKafkaReceiver"
	/// Parameters = {
	///   "bootstrapServers": "localhost:9092",
	///   "topic": "user-deletions",
	///   "clientId": "ChangeTrackerClient",
	///   "acks": "all"
	/// }
	/// </para>
	/// </remarks>
	public JsonNode? Parameters { get; set; }

}
