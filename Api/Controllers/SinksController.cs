using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for managing sink plugins
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SinksController : ControllerBase
{
    private readonly CoreService _coreService;

    public SinksController(CoreService coreService)
    {
        _coreService = coreService;
    }

    /// <summary>
    /// Get all loaded sink plugins
    /// </summary>
    /// <returns>List of loaded sink plugins</returns>
    [HttpGet]
    public IActionResult GetSinks()
    {
        return Ok(_coreService.GetLoadedSinks());
    }

    /// <summary>
    /// Load a sink plugin by name
    /// </summary>
    /// <param name="pluginName">Name of the plugin to load</param>
    /// <returns>Result of the operation</returns>
    [HttpPost]
    public async Task<IActionResult> LoadSink([FromQuery] string pluginName)
    {
        await _coreService.LoadSinkPluginAsync(pluginName);
        return Ok();
    }
}