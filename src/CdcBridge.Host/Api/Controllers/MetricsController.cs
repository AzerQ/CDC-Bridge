using CdcBridge.Host.Api.DTOs;
using CdcBridge.Host.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Host.Api.Controllers;

/// <summary>
/// API для получения метрик системы CDC Bridge.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(MetricsService metricsService, ILogger<MetricsController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Получает общие метрики системы.
    /// </summary>
    /// <returns>Метрики системы.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MetricsDto>> GetMetrics()
    {
        try
        {
            var metrics = await _metricsService.GetMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return StatusCode(500, "Internal server error");
        }
    }
}
