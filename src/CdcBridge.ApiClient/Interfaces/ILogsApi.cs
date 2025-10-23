using Refit;

namespace CdcBridge.ApiClient.Interfaces;

/// <summary>
/// Refit interface для работы с Logs API.
/// </summary>
public interface ILogsApi
{
    /// <summary>
    /// Получает список логов с фильтрацией и пагинацией.
    /// </summary>
    [Get("/api/logs")]
    [Headers("Authorization: Bearer")]
    Task<PagedResultDto<LogEntryDto>> GetLogsAsync([Query] LogQueryDto query);
}

/// <summary>
/// DTO для запроса логов с фильтрацией и пагинацией.
/// </summary>
public class LogQueryDto
{
    public string? Level { get; set; }
    public string? MessageSearch { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

/// <summary>
/// DTO для записи лога.
/// </summary>
public class LogEntryDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Level { get; set; }
    public required string Message { get; set; }
    public string? Exception { get; set; }
    public string? Properties { get; set; }
}
