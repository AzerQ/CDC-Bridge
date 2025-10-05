using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ICdcBridgeStorage = CdcBridge.Persistence.Abstractions.ICdcBridgeStorage;

namespace CdcBridge.Service.Workers.Subworkers;

/// <summary>
/// Воркер, отвечающий за доставку буферизованных изменений одному получателю (Receiver).
/// </summary>
public class ReceiverWorker(
    ILogger<ReceiverWorker> logger,
    Receiver receiverConfig,
    ICdcBridgeStorage storage,
    ComponentFactory componentFactory,
    Filter? filterConfig,
    Transformer? transformerConfig,
    IOptions<ReceiverWorkerConfiguration> workerConfig)
{
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ReceiverWorker for '{ReceiverName}' started.", receiverConfig.Name);

        var receiver = componentFactory.GetInstance<IReceiver>(receiverConfig.Type);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingChanges = await storage.GetPendingChangesAsync(receiverConfig.Name, receiverConfig.TrackingInstance, workerConfig.Value.BatchSize);

                foreach (var bufferedChange in pendingChanges)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var change = bufferedChange.Change;
                    bool success = false;
                    string? error = null;

                    try
                    {
                        // 1. Фильтрация
                        if (filterConfig != null)
                        {
                            var filter = componentFactory.GetInstance<IFilter>(filterConfig.Type);
                            if (!filter.IsMatch(change, filterConfig.Parameters))
                            {
                                // Если событие отфильтровано, считаем его "успешно" обработанным,
                                // чтобы оно не висело в очереди.
                                await storage.UpdateChangeStatusAsync(bufferedChange.Id, receiverConfig.TrackingInstance, receiverConfig.Name, true, "Filtered out");
                                continue;
                            }
                        }

                        // 2. Трансформация
                        if (transformerConfig != null)
                        {
                            var transformer = componentFactory.GetInstance<ITransformer>(transformerConfig.Type);
                            change.Data.TransformedData = transformer.Transform(change, transformerConfig.Parameters);
                        }

                        // 3. Отправка (здесь можно в будущем добавить Polly)
                        var result = await receiver.SendAsync(change, receiverConfig.Parameters);
                        success = result.Status == Core.Models.ReceiverProcessStatus.Success;
                        error = result.ErrorDescription;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        error = ex.Message;
                        logger.LogError(ex, "Unhandled exception while processing change {ChangeId} for receiver {ReceiverName}", bufferedChange.Id, receiverConfig.Name);
                    }

                    await storage.UpdateChangeStatusAsync(bufferedChange.Id, receiverConfig.TrackingInstance, receiverConfig.Name, success, error);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReceiverWorker for '{ReceiverName}'. Will retry after delay.", receiverConfig.Name);
            }
            
            await Task.Delay(workerConfig.Value.PollingIntervalMs, stoppingToken);
        }
        logger.LogInformation("ReceiverWorker for '{ReceiverName}' stopped.", receiverConfig.Name);
    }
}