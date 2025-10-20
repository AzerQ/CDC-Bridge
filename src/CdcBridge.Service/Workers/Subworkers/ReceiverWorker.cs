using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
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
                    long? deliveryTimeMs = null;

                    // Get the delivery status for this receiver
                    var deliveryStatus = bufferedChange.DeliveryStatuses.FirstOrDefault(s => s.ReceiverName == receiverConfig.Name);
                    
                    // Check if retry limit is exceeded
                    if (deliveryStatus != null && receiverConfig.RetryCount > 0 && deliveryStatus.AttemptCount >= receiverConfig.RetryCount)
                    {
                        logger.LogWarning("Change {ChangeId} for receiver {ReceiverName} exceeded retry limit ({RetryCount}). Marking as failed.",
                            bufferedChange.Id, receiverConfig.Name, receiverConfig.RetryCount);
                        
                        await storage.UpdateChangeStatusAsync(bufferedChange.Id, receiverConfig.TrackingInstance, receiverConfig.Name, 
                            false, $"Exceeded retry limit of {receiverConfig.RetryCount} attempts");
                        continue;
                    }

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

                        // 3. Отправка с измерением времени
                        var stopwatch = Stopwatch.StartNew();
                        var result = await receiver.SendAsync(change, receiverConfig.Parameters);
                        stopwatch.Stop();
                        
                        deliveryTimeMs = stopwatch.ElapsedMilliseconds;
                        success = result.Status == Core.Models.ReceiverProcessStatus.Success;
                        error = result.ErrorDescription;
                        
                        if (success)
                        {
                            logger.LogInformation("Successfully sent change {ChangeId} to receiver {ReceiverName} in {DeliveryTime}ms",
                                bufferedChange.Id, receiverConfig.Name, deliveryTimeMs);
                        }
                        else
                        {
                            logger.LogWarning("Failed to send change {ChangeId} to receiver {ReceiverName}. Attempt {AttemptCount}. Error: {Error}",
                                bufferedChange.Id, receiverConfig.Name, deliveryStatus?.AttemptCount ?? 0 + 1, error);
                        }
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        error = ex.Message;
                        logger.LogError(ex, "Unhandled exception while processing change {ChangeId} for receiver {ReceiverName}. Attempt {AttemptCount}",
                            bufferedChange.Id, receiverConfig.Name, deliveryStatus?.AttemptCount ?? 0 + 1);
                    }

                    await storage.UpdateChangeStatusAsync(bufferedChange.Id, receiverConfig.TrackingInstance, receiverConfig.Name, success, error, deliveryTimeMs);
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