using CdcBridge.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace CdcBridge.Host.Api.Controllers;

/// <summary>
/// API для просмотра конфигурации системы CDC Bridge.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly ICdcConfigurationContext _configContext;

    public ConfigurationController(ICdcConfigurationContext configContext)
    {
     _configContext = configContext;
    }

    /// <summary>
    /// Получает полную конфигурацию системы.
    /// </summary>
    /// <returns>Конфигурация системы.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetConfiguration()
    {
 var settings = _configContext.CdcSettings;
        return Ok(settings);
    }

    /// <summary>
    /// Получает список tracking instances.
    /// </summary>
    /// <returns>Список tracking instances.</returns>
    [HttpGet("tracking-instances")]
 [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetTrackingInstances()
    {
        var trackingInstances = _configContext.CdcSettings.TrackingInstances;
     return Ok(trackingInstances);
    }

    /// <summary>
    /// Получает список receivers.
    /// </summary>
    /// <returns>Список receivers.</returns>
    [HttpGet("receivers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetReceivers()
    {
        var receivers = _configContext.CdcSettings.Receivers;
 return Ok(receivers);
    }
}
