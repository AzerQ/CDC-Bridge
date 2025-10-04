using System.Text.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace CdcBridge.Configuration.Converters;

// Упрощенный подход с кастомной десериализацией
public class JsonElementConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(JsonElement);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer nestedObjectDeserializer)
    {
        // Для чтения YAML в JsonElement
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(parser);
        
        // Конвертируем объект в JsonElement
        var jsonString = JsonSerializer.Serialize(yamlObject);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer nestedObjectSerializer)
    {
        // Для записи JsonElement в YAML
        if (value is JsonElement element)
        {
            var jsonString = JsonSerializer.Serialize(element);
            var yamlObject = JsonSerializer.Deserialize<object>(jsonString);
            
            var serializer = new SerializerBuilder().Build();
            serializer.Serialize(emitter, yamlObject);
        }
    }
}