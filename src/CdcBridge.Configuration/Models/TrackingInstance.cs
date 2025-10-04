namespace CdcBridge.Configuration.Models;

public class TrackingInstance
{
    public string SourceTable { get; set; } = string.Empty;
    public List<string> CapturedColumns { get; set; } = new();
    public string? Description { get; set; }
    public string Connection { get; set; } = string.Empty;
    public bool Active { get; set; }
    public int CheckIntervalInSeconds { get; set; }
}