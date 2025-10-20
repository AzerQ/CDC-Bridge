using CdcBridge.Api.DTOs;
using CdcBridge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Api.Controllers;

/// <summary>
/// API для работы с событиями изменений данных.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly EventsService _eventsService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(EventsService eventsService, ILogger<EventsController> logger)
    {
        _eventsService = eventsService;
        _logger = logger;
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
        try
        {
            var events = await _eventsService.GetEventsAsync(query);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Получает детальную информацию о конкретном событии.
    /// </summary>
    /// <param name="id">Идентификатор события.</param>
    /// <returns>Информация о событии.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        try
        {
            var eventDto = await _eventsService.GetEventByIdAsync(id);
            if (eventDto == null)
            {
                return NotFound();
            }
            return Ok(eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event {EventId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
