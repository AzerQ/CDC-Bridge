using CdcWebhookReceiver.Models;
using CdcWebhookReceiver.Services;
using Microsoft.AspNetCore.Mvc;

namespace CdcWebhookReceiver.Controllers;

/// <summary>
/// Controller for retrieving CDC events
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;
    private readonly IChangeDataCaptureService _cdcService;

    public EventsController(ILogger<EventsController> logger, IChangeDataCaptureService cdcService)
    {
        _logger = logger;
        _cdcService = cdcService;
    }

    /// <summary>
    /// Gets all CDC events
    /// </summary>
    /// <returns>List of all CDC events</returns>
    [HttpGet]
    public ActionResult<IEnumerable<CdcEvent>> GetAllEvents()
    {
        try
        {
            var events = _cdcService.GetAllEvents();
            _logger.LogInformation($"Retrieved {events.Count} CDC events");
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CDC events");
            return StatusCode(500, "Error retrieving CDC events");
        }
    }

    /// <summary>
    /// Gets CDC events filtered by table name
    /// </summary>
    /// <param name="tableName">Table name to filter by</param>
    /// <returns>Filtered list of CDC events</returns>
    [HttpGet("table/{tableName}")]
    public ActionResult<IEnumerable<CdcEvent>> GetEventsByTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return BadRequest("Table name cannot be empty");
        }

        try
        {
            var events = _cdcService.GetEventsByTable(tableName);
            _logger.LogInformation($"Retrieved {events.Count} CDC events for table {tableName}");
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving CDC events for table {tableName}");
            return StatusCode(500, $"Error retrieving CDC events for table {tableName}");
        }
    }

    /// <summary>
    /// Gets CDC events filtered by operation type
    /// </summary>
    /// <param name="operation">Operation type to filter by (insert, update, delete)</param>
    /// <returns>Filtered list of CDC events</returns>
    [HttpGet("operation/{operation}")]
    public ActionResult<IEnumerable<CdcEvent>> GetEventsByOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            return BadRequest("Operation cannot be empty");
        }

        try
        {
            var events = _cdcService.GetEventsByOperation(operation);
            _logger.LogInformation($"Retrieved {events.Count} CDC events for operation {operation}");
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving CDC events for operation {operation}");
            return StatusCode(500, $"Error retrieving CDC events for operation {operation}");
        }
    }
}