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
    public JsonPathFilter(string name)
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
        if (parameters.ValueKind == JsonValueKind.Null)
            throw new ArgumentNullException(nameof(parameters));

        try
        {
            // Получаем JsonPath выражение из параметров
            var expression = GetJsonPathExpression(parameters);
            
            // Сериализуем TrackedChange в JToken для оценки JsonPath
            var changeJson = SerializeTrackedChange(trackedChange);
            var changeJToken = JToken.FromObject(changeJson);
            
            // Выполняем JsonPath выражение
            var results = JToken.Parse(expression) is JArray array 
                ? EvaluateJsonPathArray(changeJToken, array) 
                : EvaluateJsonPathSingle(changeJToken, expression);
            
            return results.Any();
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid JsonPath expression in filter '{Name}'.", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Error evaluating JsonPath expression in filter '{Name}'.", ex);
        }
    }

    private static string GetJsonPathExpression(JsonElement parameters)
    {
        if (parameters.TryGetProperty("expression", out var expressionProp) &&
            expressionProp.ValueKind == JsonValueKind.String)
        {
            return expressionProp.GetString() ?? 
                   throw new InvalidOperationException("JsonPath expression cannot be null or empty.");
        }

        throw new InvalidOperationException(
            "JsonPath filter requires 'expression' parameter of type string.");
    }

    private static object SerializeTrackedChange(TrackedChange change)
    {
        return new
        {
            change.ChangeType,
            change.TrackingInstance,
            change.CreatedAt,
            change.RowLabel,
            change.Data
        };
    }

    private static IEnumerable<JToken> EvaluateJsonPathArray(JToken changeJToken, JArray expressions)
    {
        foreach (var expressionToken in expressions)
        {
            var expression = expressionToken.ToString();
            var matches = changeJToken.SelectTokens(expression);
            foreach (var match in matches)
            {
                yield return match;
            }
        }
    }

    private static IEnumerable<JToken> EvaluateJsonPathSingle(JToken changeJToken, string expression)
    {
        return changeJToken.SelectTokens(expression);
    }
}