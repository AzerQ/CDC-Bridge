using System.Text.RegularExpressions;

namespace CdcBridge.Configuration.Preprocessing;

/// <summary>
/// Оркестрирует процесс предварительной обработки YAML-строки,
/// используя все зарегистрированные IYamlPreprocessor.
/// </summary>
public class YamlProcessor
{
    private readonly IEnumerable<IYamlPreprocessor> _preprocessors;
    private readonly Regex _macroRegex;

    public YamlProcessor(IEnumerable<IYamlPreprocessor> preprocessors)
    {
        _preprocessors = preprocessors.ToList();
        
        // Динамически строим регулярное выражение, чтобы оно находило все известные макросы
        var macroNames = string.Join("|", _preprocessors.Select(p => Regex.Escape(p.MacroName)));
        _macroRegex = new Regex($@"\b({macroNames})\s*\(\s*[""']([^""']+)[""']\s*\)", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Обрабатывает YAML-строку, заменяя все макросы.
    /// </summary>
    public string Process(string yamlContent, string yamlFilePath)
    {
        var context = new PreprocessorContext(BasePath: Path.GetDirectoryName(yamlFilePath)!);
        
        return _macroRegex.Replace(yamlContent, match =>
        {
            var macroName = match.Groups[1].Value;
            var argument = match.Groups[2].Value;

            var preprocessor = _preprocessors.FirstOrDefault(p => p.MacroName.Equals(macroName, StringComparison.OrdinalIgnoreCase));

            return preprocessor?.Process(argument, context) ?? match.Value; // Если препроцессор не найден, оставляем как есть
        });
    }
}