using CdcBridge.Configuration;
using CdcBridge.Persistence.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CdcBridge.Service.Workers;

/// <summary>
/// Фоновый сервис, который периодически очищает старые, успешно обработанные
/// события из буфера, чтобы база данных не разрасталась бесконечно.
/// </summary>
public class CleanupWorker : BackgroundService
{
    private readonly ILogger<CleanupWorker> _logger;
    private readonly ICdcBridgeStorage _storage;
    private readonly ICdcConfigurationContext _configContext;
    // TODO: Сделать интервал настраиваемым
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(3); 
    private readonly TimeSpan _timeToLive = TimeSpan.FromHours(24); // Время жизни по умолчанию

    public CleanupWorker(ILogger<CleanupWorker> logger, ICdcBridgeStorage storage, ICdcConfigurationContext configContext)
    {
        _logger = logger;
        _storage = storage;
        _configContext = configContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CleanupWorker started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting periodic cleanup run.");
            foreach (var ti in _configContext.CdcSettings.TrackingInstances)
            {
                try
                {
                    // TTL можно будет вынести в конфиг TrackingInstance
                    await _storage.CleanupAsync(ti.Name, _timeToLive);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cleanup for tracking instance '{InstanceName}'", ti.Name);
                }
            }
            
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("CleanupWorker stopped.");
    }
}