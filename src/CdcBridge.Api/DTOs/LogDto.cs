namespace CdcBridge.Api.DTOs;

/// <summary>
/// DTO для запроса логов с фильтрацией и пагинацией.
/// </summary>
public class LogQueryDto
{
    /// <summary>
    /// Уровень логирования (Information, Warning, Error, etc.).
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// Текстовый поиск в сообщении лога.
    /// </summary>
    public string? MessageSearch { get; set; }

    /// <summary>
    /// Начало диапазона времени.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Конец диапазона времени.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Номер страницы (начиная с 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Размер страницы.
    /// </summary>
    public int PageSize { get; set; } = 100;
}

/// <summary>
/// DTO для записи лога.
/// </summary>
public class LogEntryDto
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Временная метка.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Уровень логирования.
    /// </summary>
    public required string Level { get; set; }

    /// <summary>
    /// Сообщение лога.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Исключение, если есть.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Дополнительные свойства в формате JSON.
    /// </summary>
    public string? Properties { get; set; }
}
