namespace CdcBridge.Configuration.Preprocessing;

/// <summary>
/// Представляет контекст, передаваемый препроцессору во время обработки.
/// </summary>
/// <param name="BasePath">Абсолютный путь к директории, где находится обрабатываемый YAML-файл.</param>
public record PreprocessorContext(string BasePath);

/// <summary>
/// Определяет контракт для препроцессора, который может обрабатывать
/// специальные макросы в YAML-файле перед десериализацией.
/// </summary>
public interface IYamlPreprocessor
{
    /// <summary>
    /// Ключевое слово (имя функции), на которое реагирует этот препроцессор.
    /// Например: "Configuration", "IncludeFileContent".
    /// </summary>
    string MacroName { get; }

    /// <summary>
    /// Обрабатывает найденный макрос и возвращает строку для замены.
    /// </summary>
    /// <param name="argument">Аргумент, переданный в макрос (содержимое в скобках и кавычках).</param>
    /// <param name="context">Контекст выполнения, содержащий, например, базовый путь файла.</param>
    /// <returns>Строка, которая будет подставлена вместо всего выражения макроса.</returns>
    string Process(string argument, PreprocessorContext context);
}