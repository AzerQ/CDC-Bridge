using System.Text.Json;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;
using Newtonsoft.Json.Linq;

namespace CdcBridge.Application.Filters;

/// <summary>
/// Фильтр на основе JsonPath выражений для отбора событий изменений.
/// Использует библиотеку Newtonsoft.Json для оценки JsonPath выражений.
/// </summary>
/// <remarks>
/// <para>
/// Фильтр проверяет, соответствует ли событие изменения заданному JsonPath выражению.
/// Поддерживает стандартный синтаксис JsonPath для фильтрации по структуре данных события.
/// </para>
/// <para>
/// Параметры конфигурации:
/// <list type="bullet">
/// <item>
/// <description><c>expression</c> - обязательное JsonPath выражение для фильтрации</description>
/// </item>
/// <item>
/// <description><c>options</c> - опциональные настройки оценки JsonPath</description>
/// </item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Пример конфигурации YAML:
/// <code>
/// filters:
///   - name: "ActiveUsersFilter"
///     type: "JsonPathFilter"
///     parameters:
///       expression: "$[?(@.data.new.status == 'active')]"
/// </code>
/// </example>
public class JsonPathFilter : IFilter
{

    /// <summary>
    /// Инициализирует новый экземпляр фильтра JsonPath.
    /// </summary>
    public JsonPathFilter()
    {
    }

    /// <summary>
    /// Уникальное имя фильтра.
    /// </summary>
    public string Name => nameof(JsonPathFilter);

    /// <summary>
    /// Проверяет, соответствует ли событие изменения заданному JsonPath выражению.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для проверки.</param>
    /// <param name="parameters">
    /// Параметры фильтра в формате JSON. Должен содержать свойство "expression" с JsonPath выражением.
    /// </param>
    /// <returns>
    /// <c>true</c>, если событие соответствует JsonPath выражению; иначе <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// trackedChange или parameters являются null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// В параметрах отсутствует обязательное свойство "expression".
    /// </exception>
    /// <exception cref="JsonException">
    /// Неверный формат JsonPath выражения или JSON данных.
    /// </exception>
    public bool IsMatch(TrackedChange trackedChange, JsonElement parameters)
    {
        if (trackedChange == null)
            throw new ArgumentNullException(nameof(trackedChange));
        if (parameters.ValueKind == JsonValueKind.Null || parameters.ValueKind == JsonValueKind.Undefined)
            throw new ArgumentNullException(nameof(parameters));

        
        string? expression = null;
        try
        {
            expression = GetJsonPathExpression(parameters);
            var changeJObject = ConvertTrackedChangeToJObject(trackedChange);

            // JsonPath-выражения, начинающиеся с `$[?`, предназначены для фильтрации массивов.
            // Мы должны обернуть наш единственный объект в массив, чтобы такое выражение работало.
            JToken tokenToQuery = expression.StartsWith("$[?") ? new JArray(changeJObject) : changeJObject;

            var results = tokenToQuery.SelectTokens(expression);
            return results.Any();
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            // Оборачиваем исключение парсинга JsonPath в наше собственное для ясности.
            throw new InvalidOperationException($"Invalid JsonPath expression: '{expression}'", ex);
        }
    }

    private static JObject ConvertTrackedChangeToJObject(TrackedChange trackedChange)
    {
        // Вручную создаем JObject, чтобы обеспечить правильную сериализацию вложенного JsonElement.
        // Это дает нам полный контроль над структурой JSON.
        var changeJObject = new JObject
        {
            ["changeType"] = trackedChange.ChangeType.ToString(),
            ["trackingInstance"] = trackedChange.TrackingInstance,
            ["createdAt"] = trackedChange.CreatedAt,
            ["rowLabel"] = trackedChange.RowLabel,
            ["data"] = new JObject
            {
                ["old"] = trackedChange.Data.Old.HasValue ? JToken.Parse(trackedChange.Data.Old.Value.GetRawText()) : null,
                ["new"] = trackedChange.Data.New.HasValue ? JToken.Parse(trackedChange.Data.New.Value.GetRawText()) : null
            }
        };
        return changeJObject;
    }

    private string GetJsonPathExpression(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("expression", out var expressionElement))
        {
            throw new InvalidOperationException($"Filter '{Name}' requires 'expression' parameter.");
        }

        var expression = expressionElement.GetString();
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new InvalidOperationException($"JsonPath expression for filter '{Name}' cannot be empty.");
        }

        return expression;
    }
}

