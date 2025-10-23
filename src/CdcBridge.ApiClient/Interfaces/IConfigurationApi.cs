using Refit;

namespace CdcBridge.ApiClient.Interfaces;

/// <summary>
/// Refit interface для работы с Configuration API.
/// </summary>
public interface IConfigurationApi
{
    /// <summary>
    /// Получает полную конфигурацию системы.
    /// </summary>
    [Get("/api/configuration")]
    [Headers("Authorization: Bearer")]
    Task<object> GetConfigurationAsync();

    /// <summary>
    /// Получает список tracking instances.
    /// </summary>
    [Get("/api/configuration/tracking-instances")]
    [Headers("Authorization: Bearer")]
    Task<List<TrackingInstanceDto>> GetTrackingInstancesAsync();

    /// <summary>
    /// Получает список receivers.
    /// </summary>
    [Get("/api/configuration/receivers")]
    [Headers("Authorization: Bearer")]
    Task<List<ReceiverDto>> GetReceiversAsync();
}

/// <summary>
/// DTO для tracking instance.
/// </summary>
public class TrackingInstanceDto
{
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string CaptureInstance { get; set; }
    public int PollingIntervalSeconds { get; set; }
    public int BatchSize { get; set; }
}

/// <summary>
/// DTO для receiver.
/// </summary>
public class ReceiverDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Url { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? RetryCount { get; set; }
}
