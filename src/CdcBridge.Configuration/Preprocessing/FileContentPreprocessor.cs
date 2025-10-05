namespace CdcBridge.Configuration.Preprocessing;

/// <summary>
/// Препроцессор для макроса `IncludeFileContent("...")`.
/// Заменяет макрос содержимым указанного файла, форматируя его как
/// многострочную строку в YAML (literal block scalar).
/// </summary>
public class FileContentPreprocessor : IYamlPreprocessor
{
    public string MacroName => "IncludeFileContent";

    public string Process(string argument, PreprocessorContext context)
    {
        // Разрешаем относительный путь файла относительно расположения YAML-конфига
        var filePath = Path.GetFullPath(Path.Combine(context.BasePath, argument));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File specified in {MacroName} macro not found.", filePath);
        }

        var fileContent = File.ReadAllText(filePath);

        // Чтобы корректно вставить многострочный текст в YAML, мы используем
        // синтаксис literal block scalar ('|').
        // Мы также должны добавить отступы к каждой строке содержимого файла.
        const string indent = "  "; // Стандартный отступ в 2 пробела
        var indentedContent = string.Join(
            Environment.NewLine,
            fileContent.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                .Select(line => indent + line)
        );

        // Возвращаем отформатированную строку. Она заменит `IncludeFileContent(...)`.
        // Важно: эта замена предполагает, что макрос находится на месте значения ключа.
        // Например: `transformation: IncludeFileContent(...)`
        return $"|\n{indentedContent}";
    }
}