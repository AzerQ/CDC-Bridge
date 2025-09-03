using CdcWebhookReceiver.Models;

namespace CdcWebhookReceiver.Services;

/// <summary>
/// Interface for CDC service operations
/// </summary>
public interface IChangeDataCaptureService
{
    /// <summary>
    /// Adds CDC events to the in-memory storage
    /// </summary>
    /// <param name="events">List of CDC events to add</param>
    void AddEvents(List<CdcEvent> events);
    
    /// <summary>
    /// Gets all CDC events from the in-memory storage
    /// </summary>
    /// <returns>List of all CDC events</returns>
    List<CdcEvent> GetAllEvents();
    
    /// <summary>
    /// Gets CDC events filtered by table name
    /// </summary>
    /// <param name="tableName">Table name to filter by</param>
    /// <returns>Filtered list of CDC events</returns>
    List<CdcEvent> GetEventsByTable(string tableName);
    
    /// <summary>
    /// Gets CDC events filtered by operation type
    /// </summary>
    /// <param name="operation">Operation type to filter by</param>
    /// <returns>Filtered list of CDC events</returns>
    List<CdcEvent> GetEventsByOperation(string operation);
    
    /// <summary>
    /// Clears all CDC events from the in-memory storage
    /// </summary>
    void ClearEvents();
}

/// <summary>
/// Service for storing and managing CDC events in memory
/// </summary>
public class ChangeDataCaptureService : IChangeDataCaptureService
{
    private readonly List<CdcEvent> _events = new();
    private readonly object _lock = new();
    
    /// <summary>
    /// Adds CDC events to the in-memory storage
    /// </summary>
    /// <param name="events">List of CDC events to add</param>
    public void AddEvents(List<CdcEvent> events)
    {
        if (events == null || !events.Any())
            return;
            
        lock (_lock)
        {
            _events.AddRange(events);
        }
    }
    
    /// <summary>
    /// Gets all CDC events from the in-memory storage
    /// </summary>
    /// <returns>List of all CDC events</returns>
    public List<CdcEvent> GetAllEvents()
    {
        lock (_lock)
        {
            return _events.OrderByDescending(e => e.ReceivedTime).ToList();
        }
    }
    
    /// <summary>
    /// Gets CDC events filtered by table name
    /// </summary>
    /// <param name="tableName">Table name to filter by</param>
    /// <returns>Filtered list of CDC events</returns>
    public List<CdcEvent> GetEventsByTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return new List<CdcEvent>();
            
        lock (_lock)
        {
            return _events
                .Where(e => e.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.ReceivedTime)
                .ToList();
        }
    }
    
    /// <summary>
    /// Gets CDC events filtered by operation type
    /// </summary>
    /// <param name="operation">Operation type to filter by</param>
    /// <returns>Filtered list of CDC events</returns>
    public List<CdcEvent> GetEventsByOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
            return new List<CdcEvent>();
            
        lock (_lock)
        {
            return _events
                .Where(e => e.Operation.Equals(operation, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.ReceivedTime)
                .ToList();
        }
    }
    
    /// <summary>
    /// Clears all CDC events from the in-memory storage
    /// </summary>
    public void ClearEvents()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }
}