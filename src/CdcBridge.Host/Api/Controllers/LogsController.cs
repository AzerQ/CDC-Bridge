using CdcBridge.Host.Api.DTOs;
using CdcBridge.Host.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Host.Api.Controllers;

/// <summary>
/// API для работы с логами системы.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly LogsService _logsService;

    public LogsController(LogsService logsService)
    {
        _logsService = logsService;
    }

    /// <summary>
    /// Получает список логов с фильтрацией и пагинацией.
    /// </summary>
    /// <param name="query">Параметры запроса.</param>
    /// <returns>Список логов.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<LogEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<LogEntryDto>>> GetLogs([FromQuery] LogQueryDto query)
    {
        var logs = await _logsService.GetLogsAsync(query);
        return Ok(logs);
    }
}
