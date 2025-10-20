using CdcBridge.Configuration;
using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using CdcBridge.Service.Workers.Subworkers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ICdcBridgeStorage = CdcBridge.Persistence.Abstractions.ICdcBridgeStorage;

namespace CdcBridge.Service.Workers;

/// <summary>
/// Главный сервис-оркестратор. Читает конфигурацию и динамически запускает
/// воркеры для каждого источника и получателя.
/// </summary>
public class CdcBridgeOrchestrator : IHostedService
{
    private readonly ILogger<CdcBridgeOrchestrator> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICdcConfigurationContext _configContext;
    private readonly List<Task> _workerTasks = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public CdcBridgeOrchestrator(ILogger<CdcBridgeOrchestrator> logger, IServiceProvider serviceProvider, ICdcConfigurationContext configContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configContext = configContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CDC Bridge Orchestrator is starting.");
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _logger.LogInformation("Initializing tracking instances...");
        // Запуск Source Workers
        foreach (var trackingInstanceConfig in _configContext.CdcSettings.TrackingInstances.Where(t => t.Active))
        {
            var (hasSourceWorkerSuccessfullyCreated, sourceWorker) = await TryMakeSourceWorker(trackingInstanceConfig);
            
            if (hasSourceWorkerSuccessfullyCreated)
                _workerTasks.Add(sourceWorker!.ExecuteAsync(_cancellationTokenSource.Token));
            else
                _logger.LogError($"Could not create source worker: {trackingInstanceConfig.Name}");
        }

        // Запуск Receiver Workers
        foreach (Receiver receiverConfig in _configContext.CdcSettings.Receivers)
        {
            var (hasReceiverWorkerSuccessfullyCreated, receiverWorker) = await TryMakeReceiverWorker(receiverConfig);
            
            if (hasReceiverWorkerSuccessfullyCreated)
                _workerTasks.Add(receiverWorker!.ExecuteAsync(_cancellationTokenSource.Token));
            else
                _logger.LogError($"Could not create receiver worker: {receiverConfig.Name}");
        }
        
        _logger.LogInformation("Started {SourceCount} source workers and {ReceiverCount} receiver workers.", 
            _configContext.CdcSettings.TrackingInstances.Count(t => t.Active), 
            _configContext.CdcSettings.Receivers.Count());
        
    }

    private async Task<(bool, SourceWorker?)> TryMakeSourceWorker(TrackingInstance trackingInstanceConfig)
    {
        var connectionConfig = _configContext.GetConnection(trackingInstanceConfig.Connection);
            
        if (connectionConfig == null)
        {
            _logger.LogError("Connection '{ConnectionName}' for tracking instance '{InstanceName}' not found. Skipping.", trackingInstanceConfig.Connection, trackingInstanceConfig.Name);
            return (false, null);
        }

        try
        {
            ICdcSource cdcSource = _serviceProvider.GetRequiredService<ComponentFactory>()
                .GetInstance<ICdcSource>(connectionConfig.Type);

            // Вызываем метод для включения отслеживания. Он идемпотентный.
            _logger.LogInformation("Ensuring CDC is enabled for tracking instance '{InstanceName}'...",
                trackingInstanceConfig);
            await cdcSource.EnableTrackingInstance(new TrackingInstanceInfo(trackingInstanceConfig, connectionConfig));
            _logger.LogInformation("CDC is ready for tracking instance '{InstanceName}'.", trackingInstanceConfig.Name);

            var sourceWorker = new SourceWorker(
                _serviceProvider.GetRequiredService<ILogger<SourceWorker>>(),
                trackingInstanceConfig,
                connectionConfig,
                _serviceProvider.GetRequiredService<ICdcBridgeStorage>(),
                cdcSource
            );

            return (true, sourceWorker);
        }

        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize and start SourceWorker for tracking instance '{InstanceName}'. This instance will not be tracked.", trackingInstanceConfig.Name);
            return (false, null);
        }
    }

    private Task<(bool, ReceiverWorker?)> TryMakeReceiverWorker(Receiver receiverConfig)
    {
        try
        {
            var filterConfig = !string.IsNullOrEmpty(receiverConfig.Filter)
                ? _configContext.GetFilter(receiverConfig.Filter)
                : null;
            var transformerConfig = !string.IsNullOrEmpty(receiverConfig.Transformer)
                ? _configContext.GetTransformer(receiverConfig.Transformer)
                : null;

            var receiverWorker = new ReceiverWorker(
                _serviceProvider.GetRequiredService<ILogger<ReceiverWorker>>(),
                receiverConfig,
                _serviceProvider.GetRequiredService<ICdcBridgeStorage>(),
                _serviceProvider.GetRequiredService<ComponentFactory>(),
                filterConfig,
                transformerConfig,
                _serviceProvider.GetRequiredService<IOptions<ReceiverWorkerConfiguration>>()
            );
            return Task.FromResult((true, receiverWorker))!;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create receiver worker: {Error}", ex.Message);
            return Task.FromResult<(bool, ReceiverWorker?)>((false, null));
        }
        
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CDC Bridge Orchestrator is stopping.");
        _cancellationTokenSource?.Cancel();
        await Task.WhenAll(_workerTasks);
        _logger.LogInformation("All workers stopped.");
    }
}