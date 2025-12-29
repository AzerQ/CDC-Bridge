using System.Text.Json;
using Serilog.Core;
using Serilog.Events;

namespace CdcBridge.Application.DI.Logging;

public class JsonElementDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        switch (value)
        {
            case JsonDocument jdoc:
                result = Destructure(jdoc.RootElement, propertyValueFactory);
                return true;
            case JsonElement jel:
                result = Destructure(jel, propertyValueFactory);
                return true;
            default:
                result = null;
                return false;
        }
    }

    private static LogEventPropertyValue Destructure(JsonElement el, ILogEventPropertyValueFactory propertyValueFactory)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                var properties = el.EnumerateObject()
                    .Select(p => new LogEventProperty(p.Name, Destructure(p.Value, propertyValueFactory)))
                    .ToList();
                return new StructureValue(properties);

            case JsonValueKind.Array:
                var elements = el.EnumerateArray()
                    .Select(v => Destructure(v, propertyValueFactory));
                return new SequenceValue(elements);

            case JsonValueKind.String:
                return new ScalarValue(el.GetString());

            case JsonValueKind.Number:
                if (el.TryGetInt32(out int i)) return new ScalarValue(i);
                if (el.TryGetDouble(out double d)) return new ScalarValue(d);
                // Fallback for other number types
                return new ScalarValue(el.GetRawText()); 

            case JsonValueKind.True:
                return new ScalarValue(true);

            case JsonValueKind.False:
                return new ScalarValue(false);

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return new ScalarValue(null);

            default:
                return new ScalarValue(el.GetRawText());
        }
    }
}