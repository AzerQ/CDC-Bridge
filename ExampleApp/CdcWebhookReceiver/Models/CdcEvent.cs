namespace CdcWebhookReceiver.Models;

/// <summary>
/// Represents a CDC event received from SQL Server
/// </summary>
public class CdcEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Type of operation (insert, update, delete)
    /// </summary>
    public string Operation { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the table that was changed
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary key of the affected row
    /// </summary>
    public string PrimaryKey { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON representation of the data before the change (null for inserts)
    /// </summary>
    public string? DataBefore { get; set; }
    
    /// <summary>
    /// JSON representation of the data after the change (null for deletes)
    /// </summary>
    public string? DataAfter { get; set; }
    
    /// <summary>
    /// Timestamp when the change occurred in the database
    /// </summary>
    public DateTime ChangeTime { get; set; }
    
    /// <summary>
    /// Timestamp when the event was received by the webhook
    /// </summary>
    public DateTime ReceivedTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a webhook payload for CDC events
/// </summary>
public class CdcWebhookPayload
{
    /// <summary>
    /// List of CDC events in the payload
    /// </summary>
    public List<CdcEvent> Events { get; set; } = new List<CdcEvent>();
}