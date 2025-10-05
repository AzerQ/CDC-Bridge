using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using Microsoft.Extensions.Logging;
using ICdcBridgeStorage = CdcBridge.Persistence.Abstractions.ICdcBridgeStorage;

namespace CdcBridge.Service.Workers.Subworkers;

/// <summary>
/// Воркер, отвечающий за доставку буферизованных изменений одному получателю (Receiver).
/// </summary>
public class ReceiverWorker
{
    private readonly ILogger<ReceiverWorker> _logger;
    private readonly Receiver _receiverConfig;
    private readonly ICdcBridgeStorage _storage;
    private readonly ComponentFactory _componentFactory;
    private readonly Filter? _filterConfig;
    private readonly Transformer? _transformerConfig;

    // TODO: Вынести в конфигурацию
    private readonly int _pollingIntervalMs = 5000;
    private readonly int _batchSize = 100;

    public ReceiverWorker(
        ILogger<ReceiverWorker> logger,
        Receiver receiverConfig,
        ICdcBridgeStorage storage,
        ComponentFactory componentFactory,
        Filter? filterConfig,
        Transformer? transformerConfig)
    {
        _logger = logger;
        _receiverConfig = receiverConfig;
        _storage = storage;
        _componentFactory = componentFactory;
        _filterConfig = filterConfig;
        _transformerConfig = transformerConfig;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReceiverWorker for '{ReceiverName}' started.", _receiverConfig.Name);

        var receiver = _componentFactory.GetInstance<IReceiver>(_receiverConfig.Type);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingChanges = await _storage.GetPendingChangesAsync(_receiverConfig.Name, _receiverConfig.TrackingInstance, _batchSize);

                foreach (var bufferedChange in pendingChanges)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var change = bufferedChange.Change;
                    bool success = false;
                    string? error = null;

                    try
                    {
                        // 1. Фильтрация
                        if (_filterConfig != null)
                        {
                            var filter = _componentFactory.GetInstance<IFilter>(_filterConfig.Type);
                            if (!filter.IsMatch(change, _filterConfig.Parameters))
                            {
                                // Если событие отфильтровано, считаем его "успешно" обработанным,
                                // чтобы оно не висело в очереди.
                                await _storage.UpdateChangeStatusAsync(bufferedChange.Id, _receiverConfig.TrackingInstance, _receiverConfig.Name, true, "Filtered out");
                                continue;
                            }
                        }

                        // 2. Трансформация
                        if (_transformerConfig != null)
                        {
                            var transformer = _componentFactory.GetInstance<ITransformer>(_transformerConfig.Type);
                            change.Data.TransformedData = transformer.Transform(change, _transformerConfig.Parameters);
                        }

                        // 3. Отправка (здесь можно в будущем добавить Polly)
                        var result = await receiver.SendAsync(change, _receiverConfig.Parameters);
                        success = result.Status == Core.Models.ReceiverProcessStatus.Success;
                        error = result.ErrorDescription;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        error = ex.Message;
                        _logger.LogError(ex, "Unhandled exception while processing change {ChangeId} for receiver {ReceiverName}", bufferedChange.Id, _receiverConfig.Name);
                    }

                    await _storage.UpdateChangeStatusAsync(bufferedChange.Id, _receiverConfig.TrackingInstance, _receiverConfig.Name, success, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReceiverWorker for '{ReceiverName}'. Will retry after delay.", _receiverConfig.Name);
            }
            
            await Task.Delay(_pollingIntervalMs, stoppingToken);
        }
        _logger.LogInformation("ReceiverWorker for '{ReceiverName}' stopped.", _receiverConfig.Name);
    }
}