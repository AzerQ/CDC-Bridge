using System.Text.Json;

namespace CdcBridge.Configuration.Models;

/// <summary>
/// Трансформер события изменений.
/// Применяется для преобразования полезной нагрузки события (record) перед отправкой получателям.
/// </summary>
public class Transformer
{
    /// <summary>
    /// Уникальное имя трансформера.
    /// Используется для ссылок из конфигурации (например, receivers[].transformer).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Произвольное описание назначения/логики трансформера.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Идентификатор экземпляра трекинга, для которого применяется данный трансформер.
    /// Рекомендуется указывать значение <c>TrackingInstance.sourceTable</c> из конфигурации.
    /// </summary>
    public required string TrackingInstance { get; set; }

    /// <summary>
    /// Тип трансформера, определяющий механизм преобразования и ожидаемую схему параметров.
    /// Примеры: <c>JSONataTransformer</c> и др.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Параметры трансформера.
    /// Состав и обязательные поля зависят от значения <see cref="Type"/>.
    /// Например, для <c>JSONataTransformer</c> ожидается свойство <c>transformation</c> с выражением.
    /// </summary>
    /// <remarks>
    /// Примеры:
    /// <para>
    /// Type = "JSONataTransformer"
    /// Parameters =
    /// { "transformation": "{ 'userId': data.new.id, 'displayName': data.new.name }" }
    /// </para>
    /// </remarks>
    /// <example>
    /// Пример конфигурации:
    /// <code language="json">
    /// {
    ///   "name": "AnalyticsServiceTransformer",
    ///   "description": "Преобразование данных для аналитики",
    ///   "trackingInstance": "users",
    ///   "type": "JSONataTransformer",
    ///   "parameters": {
    ///     "transformation": "{ 'userId': data.new.id, 'displayName': data.new.name }"
    ///   }
    /// }
    /// </code>
    /// </example>
    public JsonElement Parameters { get; set; }
}