using CdcBridge.Host.Api.DTOs;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// Интерфейс сервиса для работы с логами, хранящимися в SQLite.
/// </summary>
public interface ILogsService
{
 /// <summary>
    /// Получает список логов с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="query">Параметры запроса с фильтрами и настройками пагинации.</param>
    /// <returns>Пагинированный результат с логами.</returns>
    Task<PagedResultDto<LogEntryDto>> GetLogsAsync(LogQueryDto query);
}
