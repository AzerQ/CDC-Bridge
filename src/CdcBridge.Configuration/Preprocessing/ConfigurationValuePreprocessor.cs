using Microsoft.Extensions.Configuration;

namespace CdcBridge.Configuration.Preprocessing;

/// <summary>
/// Препроцессор для макроса `Configuration("...")`.
/// Заменяет макрос значением из IConfiguration приложения.
/// </summary>
public class ConfigurationValuePreprocessor(IConfiguration configuration) : IYamlPreprocessor
{
    public string MacroName => "Configuration";

    public string Process(string argument, PreprocessorContext context)
    {
        var value = configuration[argument];
        if (value == null)
        {
            throw new InvalidOperationException($"Configuration key '{argument}' specified in YAML was not found in the application configuration.");
        }
        return value;
    }
}