using CdcBridge.Configuration;
using CdcBridge.Core.Abstractions;
using CdcBridge.Service.Workers.Subworkers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CDC Bridge Orchestrator is starting.");
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Запуск Source Workers
        foreach (var tiConfig in _configContext.CdcSettings.TrackingInstances.Where(t => t.Active))
        {
            var worker = new SourceWorker(
                _serviceProvider.GetRequiredService<ILogger<SourceWorker>>(),
                tiConfig,
                _serviceProvider.GetRequiredService<ICdcBridgeStorage>(),
                _serviceProvider.GetRequiredService<ComponentFactory>().GetInstance<ICdcSource>(_configContext.GetConnection(tiConfig.Connection)!.Type)
            );
            _workerTasks.Add(worker.ExecuteAsync(_cancellationTokenSource.Token));
        }

        // Запуск Receiver Workers
        foreach (var receiverConfig in _configContext.CdcSettings.Receivers)
        {
            var filterConfig = !string.IsNullOrEmpty(receiverConfig.Filter) ? _configContext.GetFilter(receiverConfig.Filter) : null;
            var transformerConfig = !string.IsNullOrEmpty(receiverConfig.Transformer) ? _configContext.GetTransformer(receiverConfig.Transformer) : null;

            var worker = new ReceiverWorker(
                 _serviceProvider.GetRequiredService<ILogger<ReceiverWorker>>(),
                 receiverConfig,
                 _serviceProvider.GetRequiredService<ICdcBridgeStorage>(),
                 _serviceProvider.GetRequiredService<ComponentFactory>(),
                 filterConfig,
                 transformerConfig
            );
            _workerTasks.Add(worker.ExecuteAsync(_cancellationTokenSource.Token));
        }
        
        _logger.LogInformation("Started {SourceCount} source workers and {ReceiverCount} receiver workers.", 
            _configContext.CdcSettings.TrackingInstances.Count(t => t.Active), 
            _configContext.CdcSettings.Receivers.Count());

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CDC Bridge Orchestrator is stopping.");
        _cancellationTokenSource?.Cancel();
        await Task.WhenAll(_workerTasks);
        _logger.LogInformation("All workers stopped.");
    }
}