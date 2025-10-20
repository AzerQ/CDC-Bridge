using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace CdcBridge.Application.Transformers;

using System.Text.Json;
using Core.Abstractions;
using Core.Models;
using Jsonata.Net.Native; // или другая реализация JSONata



/// <summary>
/// Трансформер на основе языка JSONata для преобразования данных событий.
/// Выполняет преобразования JSON данных с использованием выражений JSONata.
/// </summary>
/// <remarks>
/// <para>
/// JSONata - это мощный язык запросов и преобразований для JSON, предоставляющий
/// SQL-подобные возможности для работы с JSON структурами.
/// </para>
/// <para>
/// Параметры конфигурации:
/// <list type="bullet">
/// <item>
/// <description><c>expression</c> - обязательное JSONata выражение для трансформации</description>
/// </item>
/// <item>
/// <description><c>options</c> - опциональные настройки выполнения JSONata</description>
/// </item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Пример конфигурации YAML:
/// <code>
/// transformers:
///   - name: "AnalyticsTransformer"
///     type: "JSONataTransformer"
///     parameters:
///       expression: |
///         {
///           "userId": data.new.id,
///           "displayName": data.new.firstName,
///           "eventType": $string(ChangeType),
///           "timestamp": CreatedAt,
///           "metadata": {
///             "trackingInstance": TrackingInstance,
///             "rowLabel": RowLabel
///           }
///         }
/// </code>
/// </example>
public class JSONataTransformer : ITransformer
{
    record JSONataTransformerParameters(string Expression);
    
    /// <summary>
    /// Инициализирует новый экземпляр трансформера JSONata.
    /// </summary>
    public JSONataTransformer()
    {
    }

    public string Name => nameof(JSONataTransformer);

    /// <summary>
    /// Преобразует событие изменения с использованием JSONata выражения.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для преобразования.</param>
    /// <param name="parameters">
    /// Параметры трансформера в формате JSON. Должен содержать свойство "expression" с JSONata выражением.
    /// </param>
    /// <returns>
    /// Преобразованные данные в формате JsonElement.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// trackedChange или parameters являются null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// В параметрах отсутствует обязательное свойство "expression" или произошла ошибка выполнения JSONata.
    /// </exception>
    public JsonElement Transform(TrackedChange trackedChange, JsonElement parameters)
    {
        if (trackedChange == null)
            throw new ArgumentNullException(nameof(trackedChange));
        
        if (parameters.ValueKind == JsonValueKind.Null)
            throw new ArgumentNullException(nameof(parameters));
        
        // Получаем JSONata выражение из параметров
        var jsonataParams = parameters.Deserialize<JSONataTransformerParameters>() ?? throw new NullReferenceException("JSONata parameters cannot be null");

        try
        {
            var expression = jsonataParams.Expression;
            
            // Сериализуем TrackedChange в JSON строку
            var inputJson = SerializeTrackedChange(trackedChange);
            
            // Выполняем JSONata преобразование
            var resultJson = EvaluateJsonata(expression, inputJson);
            
            // Десериализуем результат обратно в JsonElement
            return DeserializeResult(resultJson);
        }
        catch (JsonataException ex)
        {
            throw new InvalidOperationException(
                $"JSONata transformation failed in transformer '{Name}'. Expression: {GetExpressionPreview(jsonataParams)}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid JSON format in transformer '{Name}'.", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Error executing JSONata transformation in transformer '{Name}'.", ex);
        }
    }

    private static string SerializeTrackedChange(TrackedChange change)
    {
        var data = new
        {
            ChangeType = change.ChangeType.ToString(),
            change.TrackingInstance,
            change.CreatedAt,
            change.RowLabel,
            change.Data
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    private static string EvaluateJsonata(string expression, string inputJson)
    {
        // Используем Jsonata.Net.Native или другую реализацию
        var jsonata = new Jsonata.Net.Native.JsonataQuery(expression);
        return jsonata.Eval(inputJson);
    }

    private static JsonElement DeserializeResult(string resultJson)
    {
        using var document = JsonDocument.Parse(resultJson);
        return document.RootElement.Clone();
    }

    private static string GetExpressionPreview(JSONataTransformerParameters parameters)
    { 
        var expr = parameters.Expression;
        return expr?.Length > 50 ? expr.Substring(0, 47) + "..." : expr ?? "null";
    }
}