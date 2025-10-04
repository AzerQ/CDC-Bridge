using System.Collections;
using System.Text.Json;

namespace CdcBridge.Application.Extensions;

public static class IDictionaryExtension
{
    public static JsonElement ToJsonElement(this IReadOnlyDictionary<string, object> dictionary)
    {
        string jsonString = JsonSerializer.Serialize(dictionary);
        using var jsonDocument = JsonDocument.Parse(jsonString);
        return jsonDocument.RootElement.Clone();
    }
}