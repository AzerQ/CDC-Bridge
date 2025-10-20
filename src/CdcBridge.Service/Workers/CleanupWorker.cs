using CdcBridge.Configuration;
using CdcBridge.Persistence.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CdcBridge.Service.Workers;

/// <summary>
/// Фоновый сервис, который периодически очищает старые, успешно обработанные
/// события из буфера, чтобы база данных не разрасталась бесконечно.
/// </summary>
public class CleanupWorker(
    ILogger<CleanupWorker> logger,
    ICdcBridgeStorage storage,
    ICdcConfigurationContext configContext,
    IOptions<CleanupWorkerConfiguration> cleanupWorkerConfiguration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CleanupWorker started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Starting periodic cleanup run.");
            foreach (var ti in configContext.CdcSettings.TrackingInstances)
            {
                try
                {
                    await storage.CleanupAsync(ti.Name, TimeSpan.FromHours(cleanupWorkerConfiguration.Value.BufferTimeToLiveHours));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during cleanup for tracking instance '{InstanceName}'", ti.Name);
                }
            }
            
            await Task.Delay(TimeSpan.FromHours(cleanupWorkerConfiguration.Value.CleanupIntervalHours), stoppingToken);
        }

        logger.LogInformation("CleanupWorker stopped.");
    }
}