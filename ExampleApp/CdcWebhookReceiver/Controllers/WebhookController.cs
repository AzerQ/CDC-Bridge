using CdcWebhookReceiver.Models;
using CdcWebhookReceiver.Services;
using Microsoft.AspNetCore.Mvc;

namespace CdcWebhookReceiver.Controllers;

/// <summary>
/// Controller for handling CDC webhook notifications
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly IChangeDataCaptureService _cdcService;

    public WebhookController(ILogger<WebhookController> logger, IChangeDataCaptureService cdcService)
    {
        _logger = logger;
        _cdcService = cdcService;
    }

    /// <summary>
    /// Receives CDC events from webhook notifications
    /// </summary>
    /// <param name="payload">The CDC webhook payload</param>
    /// <returns>Action result</returns>
    [HttpPost]
    public IActionResult ReceiveEvents([FromBody] CdcWebhookPayload payload)
    {
        if (payload == null || payload.Events == null || !payload.Events.Any())
        {
            return BadRequest("Payload cannot be empty");
        }

        try
        {
            _logger.LogInformation($"Received {payload.Events.Count} CDC events");
            
            // Добавление событий в хранилище
            _cdcService.AddEvents(payload.Events);
            
            // Логирование полученных событий
            foreach (var cdcEvent in payload.Events)
            {
                _logger.LogInformation($"CDC Event: {cdcEvent.Operation} on {cdcEvent.TableName}, PK: {cdcEvent.PrimaryKey}, Time: {cdcEvent.ChangeTime}");
            }

            return Ok(new { message = $"Successfully processed {payload.Events.Count} CDC events" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CDC events");
            return StatusCode(500, "Error processing CDC events");
        }
    }

    /// <summary>
    /// Clears all CDC events from memory
    /// </summary>
    /// <returns>Action result</returns>
    [HttpDelete]
    public IActionResult ClearEvents()
    {
        try
        {
            _cdcService.ClearEvents();
            _logger.LogInformation("All CDC events cleared");
            return Ok(new { message = "All CDC events cleared" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing CDC events");
            return StatusCode(500, "Error clearing CDC events");
        }
    }
}