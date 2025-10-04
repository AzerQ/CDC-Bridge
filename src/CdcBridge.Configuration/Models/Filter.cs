using System.Text.Json;

namespace CdcBridge.Configuration.Models;

public class Filter
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TrackingInstance { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public JsonElement Parameters { get; set; } = new();
}