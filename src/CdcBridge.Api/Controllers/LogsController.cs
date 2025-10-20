using CdcBridge.Api.DTOs;
using CdcBridge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Api.Controllers;

/// <summary>
/// API для работы с логами системы.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly LogsService _logsService;
    private readonly ILogger<LogsController> _logger;

    public LogsController(LogsService logsService, ILogger<LogsController> logger)
    {
        _logsService = logsService;
        _logger = logger;
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
        try
        {
            var logs = await _logsService.GetLogsAsync(query);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            return StatusCode(500, "Internal server error");
        }
    }
}
