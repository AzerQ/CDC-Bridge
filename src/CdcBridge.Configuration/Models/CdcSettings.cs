namespace CdcBridge.Configuration.Models;

public class CdcSettings
{
    public List<Connection> Connections { get; set; } = new();
    public List<TrackingInstance> TrackingInstances { get; set; } = new();
    public List<Filter> Filters { get; set; } = new();
    public List<Transformer> Transformers { get; set; } = new();
    public List<Receiver> Receivers { get; set; } = new();
}