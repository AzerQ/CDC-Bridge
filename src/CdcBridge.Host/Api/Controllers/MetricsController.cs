using CdcBridge.Host.Api.DTOs;
using CdcBridge.Host.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Host.Api.Controllers;

/// <summary>
/// API для получения метрик системы CDC Bridge.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly MetricsService _metricsService;

    public MetricsController(MetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    /// <summary>
    /// Получает общие метрики системы.
    /// </summary>
    /// <returns>Метрики системы.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(MetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MetricsDto>> GetMetrics()
    {
        var metrics = await _metricsService.GetMetricsAsync();
        return Ok(metrics);
    }
}
