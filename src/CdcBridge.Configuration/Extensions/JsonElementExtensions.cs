using System.Text.Json;

namespace CdcBridge.Configuration.Extensions;

public static class JsonElementExtensions
{
    public static bool HasProperty(this JsonElement element, string propertyName) => element.TryGetProperty(propertyName, out _);
    
    public static string? GetStringProperty(this JsonElement element, string propertyName)
    {
        bool hasProperty = element.TryGetProperty(propertyName, out var elementValue);
        return hasProperty ? elementValue.GetString() : null;
    }
}