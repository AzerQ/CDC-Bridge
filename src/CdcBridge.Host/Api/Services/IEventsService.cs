using CdcBridge.Host.Api.DTOs;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// Интерфейс сервиса для работы с событиями изменений данных.
/// </summary>
public interface IEventsService
{
    /// <summary>
    /// Получает список событий с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="query">Параметры запроса с фильтрами и настройками пагинации.</param>
    /// <returns>Пагинированный результат с событиями.</returns>
    Task<PagedResultDto<EventDto>> GetEventsAsync(EventQueryDto query);

    /// <summary>
    /// Получает детальную информацию о конкретном событии.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Детальная информация о событии или null, если событие не найдено.</returns>
    Task<EventDto?> GetEventByIdAsync(Guid id);
}
