using CdcBridge.Host.Api.DTOs;
using CdcBridge.Host.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Host.Api.Controllers;

/// <summary>
/// API для работы с событиями изменений данных.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;

    public EventsController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    /// <summary>
    /// Получает список событий с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="query">Параметры запроса.</param>
    /// <returns>Список событий.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<EventDto>>> GetEvents([FromQuery] EventQueryDto query)
    {
        var events = await _eventsService.GetEventsAsync(query);
        return Ok(events);
    }

    /// <summary>
    /// Получает детальную информацию о конкретном событии.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Детальная информация о событии.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        var eventDto = await _eventsService.GetEventByIdAsync(id);
        
        if (eventDto == null)
        {
            return NotFound();
        }
        
        return Ok(eventDto);
    }
}
