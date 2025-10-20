using CdcBridge.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Api.Controllers;

/// <summary>
/// API для просмотра конфигурации системы CDC Bridge.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly ICdcConfigurationContext _configContext;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(ICdcConfigurationContext configContext, ILogger<ConfigurationController> logger)
    {
        _configContext = configContext;
        _logger = logger;
    }

    /// <summary>
    /// Получает полную конфигурацию системы.
    /// </summary>
    /// <returns>Конфигурация системы.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetConfiguration()
    {
        try
        {
            var settings = _configContext.CdcSettings;
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Получает список tracking instances.
    /// </summary>
    /// <returns>Список tracking instances.</returns>
    [HttpGet("tracking-instances")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetTrackingInstances()
    {
        try
        {
            var trackingInstances = _configContext.CdcSettings.TrackingInstances;
            return Ok(trackingInstances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tracking instances");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Получает список receivers.
    /// </summary>
    /// <returns>Список receivers.</returns>
    [HttpGet("receivers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetReceivers()
    {
        try
        {
            var receivers = _configContext.CdcSettings.Receivers;
            return Ok(receivers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receivers");
            return StatusCode(500, "Internal server error");
        }
    }
}
