namespace CdcGenerator.Configuration;

/// <summary>
/// Application settings class
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Database connection settings
    /// </summary>
    public ConnectionStrings ConnectionStrings { get; set; } = new();
    
    /// <summary>
    /// Database settings
    /// </summary>
    public DatabaseSettings DatabaseSettings { get; set; } = new();
    
    /// <summary>
    /// Data generation settings
    /// </summary>
    public DataGenerationSettings DataGenerationSettings { get; set; } = new();
}

/// <summary>
/// Connection strings settings
/// </summary>
public class ConnectionStrings
{
    /// <summary>
    /// Default connection string to the application database
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection string to the master database
    /// </summary>
    public string MasterConnection { get; set; } = string.Empty;
}

/// <summary>
/// Database settings
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Table name for CDC tracking
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    
    /// <summary>
    /// CDC capture instance name
    /// </summary>
    public string CaptureInstance { get; set; } = string.Empty;
}

/// <summary>
/// Data generation settings
/// </summary>
public class DataGenerationSettings
{
    /// <summary>
    /// Number of customers to generate
    /// </summary>
    public int NumberOfCustomers { get; set; } = 10;
    
    /// <summary>
    /// Number of customers to update
    /// </summary>
    public int NumberOfCustomersToUpdate { get; set; } = 3;
}