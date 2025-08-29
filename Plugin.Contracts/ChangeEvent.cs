namespace Plugin.Contracts;

/// <summary>
/// Represents a change event from the database.
/// </summary>
public class ChangeEvent
{
    /// <summary>
    /// The table where the change occurred.
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// The type of change (Insert, Update, Delete).
    /// </summary>
    public ChangeType Type { get; set; }

    /// <summary>
    /// The old data before the change (for Update and Delete).
    /// </summary>
    public object OldData { get; set; }

    /// <summary>
    /// The new data after the change (for Insert and Update).
    /// </summary>
    public object NewData { get; set; }

    /// <summary>
    /// The timestamp of the event.
    /// </summary>
    public DateTimeOffset EventTime { get; set; }
}