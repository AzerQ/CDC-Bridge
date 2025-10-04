using System.Text.Json;

namespace CdcBridge.Configuration.Models;

public class Receiver
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TrackingInstance { get; set; } = string.Empty;
    public string? Filter { get; set; }
    public string? Transformer { get; set; }
    public string Type { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public JsonElement Parameters { get; set; } = new();
}