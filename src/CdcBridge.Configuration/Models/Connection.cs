namespace CdcBridge.Configuration.Models;

public class Connection
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool Active { get; set; }
}