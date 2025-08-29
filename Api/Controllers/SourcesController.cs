using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for managing source plugins
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SourcesController : ControllerBase
{
    private readonly CoreService _coreService;

    public SourcesController(CoreService coreService)
    {
        _coreService = coreService;
    }

    /// <summary>
    /// Get all loaded source plugins
    /// </summary>
    /// <returns>List of loaded source plugins</returns>
    [HttpGet]
    public IActionResult GetSources()
    {
        return Ok(_coreService.GetLoadedSources());
    }

    /// <summary>
    /// Load a source plugin by name
    /// </summary>
    /// <param name="pluginName">Name of the plugin to load</param>
    /// <returns>Result of the operation</returns>
    [HttpPost]
    public async Task<IActionResult> LoadSource([FromQuery] string pluginName)
    {
        await _coreService.LoadSourcePluginAsync(pluginName);
        return Ok();
    }
}