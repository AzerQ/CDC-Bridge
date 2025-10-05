using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;
using Microsoft.Extensions.Logging;
using ICdcBridgeStorage = CdcBridge.Persistence.Abstractions.ICdcBridgeStorage;

namespace CdcBridge.Service.Workers.Subworkers;

/// <summary>
/// Воркер, отвечающий за опрос одного источника данных (TrackingInstance)
/// и сохранение полученных изменений в буфер.
/// </summary>
public class SourceWorker(
    ILogger<SourceWorker> logger,
    TrackingInstance trackingInstanceConfig,
    ICdcBridgeStorage storage,
    ICdcSource cdcSource)
{
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SourceWorker for '{InstanceName}' started.", trackingInstanceConfig.Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var lastRowLabel = await storage.GetLastProcessedRowLabelAsync(trackingInstanceConfig.Name);
                var cdcRequest = new CdcRequest { LastRowFlag = lastRowLabel };

                var changes = (await cdcSource.GetChanges(trackingInstanceConfig, cdcRequest)).ToList();

                if (changes.Any())
                {
                    await storage.AddChangesToBufferAsync(changes);
                    var newLastRowLabel = changes.Last().RowLabel;
                    await storage.SaveLastProcessedRowLabelAsync(trackingInstanceConfig.Name, newLastRowLabel);
                    
                    logger.LogInformation("SourceWorker for '{InstanceName}' buffered {Count} new changes. Last row label: {Label}",
                        trackingInstanceConfig.Name, changes.Count, newLastRowLabel);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SourceWorker for '{InstanceName}'. Will retry after delay.", trackingInstanceConfig.Name);
            }

            await Task.Delay(trackingInstanceConfig.CheckIntervalInSeconds * 1000, stoppingToken);
        }

        logger.LogInformation("SourceWorker for '{InstanceName}' stopped.", trackingInstanceConfig.Name);
    }
}