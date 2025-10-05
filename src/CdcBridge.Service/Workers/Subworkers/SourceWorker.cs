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
public class SourceWorker
{
    private readonly ILogger<SourceWorker> _logger;
    private readonly TrackingInstance _trackingInstanceConfig;
    private readonly ICdcBridgeStorage _storage;
    private readonly ICdcSource _cdcSource;

    public SourceWorker(
        ILogger<SourceWorker> logger,
        TrackingInstance trackingInstanceConfig,
        ICdcBridgeStorage storage,
        ICdcSource cdcSource)
    {
        _logger = logger;
        _trackingInstanceConfig = trackingInstanceConfig;
        _storage = storage;
        _cdcSource = cdcSource;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SourceWorker for '{InstanceName}' started.", _trackingInstanceConfig.Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var lastRowLabel = await _storage.GetLastProcessedRowLabelAsync(_trackingInstanceConfig.Name);
                var cdcRequest = new CdcRequest { LastRowFlag = lastRowLabel };

                var changes = (await _cdcSource.GetChanges(_trackingInstanceConfig, cdcRequest)).ToList();

                if (changes.Any())
                {
                    await _storage.AddChangesToBufferAsync(changes);
                    var newLastRowLabel = changes.Last().RowLabel;
                    await _storage.SaveLastProcessedRowLabelAsync(_trackingInstanceConfig.Name, newLastRowLabel);
                    
                    _logger.LogInformation("SourceWorker for '{InstanceName}' buffered {Count} new changes. Last row label: {Label}",
                        _trackingInstanceConfig.Name, changes.Count, newLastRowLabel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SourceWorker for '{InstanceName}'. Will retry after delay.", _trackingInstanceConfig.Name);
            }

            await Task.Delay(_trackingInstanceConfig.CheckIntervalInSeconds * 1000, stoppingToken);
        }

        _logger.LogInformation("SourceWorker for '{InstanceName}' stopped.", _trackingInstanceConfig.Name);
    }
}