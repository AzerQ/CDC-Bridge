using System.Text.Json;

namespace CdcBridge.Configuration.Models;

public class Transformer
{
    public string Name { get; set; } = string.Empty;
    public string? TrackingInstance { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public JsonElement Parameters { get; set; } = new();
}