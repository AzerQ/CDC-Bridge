using CdcBridge.Configuration.Models;

namespace CdcBridge.Configuration;

using System.Text.Json;

public class CdcConfigurationContext : ICdcConfigurationContext
{
    private readonly Dictionary<string, Connection> _connections;
    private readonly Dictionary<string, TrackingInstance> _trackingInstances;
    private readonly Dictionary<string, Filter> _filters;
    private readonly Dictionary<string, Transformer> _transformers;
    private readonly Dictionary<string, Receiver> _receivers;
    private readonly ILookup<string, TrackingInstance> _trackingInstancesByConnection;
    private readonly ILookup<string, Filter> _filtersByTrackingInstance;
    private readonly ILookup<string, Transformer> _transformersByTrackingInstance;
    private readonly ILookup<string, Receiver> _receiversByTrackingInstance;
    
    public CdcSettings CdcSettings { get; }

    public CdcConfigurationContext(CdcSettings settings)
    {
        CdcSettings = settings ?? throw new ArgumentNullException(nameof(settings));

        // Инициализация словарей для быстрого поиска
        _connections = settings.Connections.ToDictionary(c => c.Name);
        _trackingInstances = settings.TrackingInstances.ToDictionary(t => t.Name);
        _filters = settings.Filters?.ToDictionary(f => f.Name) ?? new Dictionary<string, Filter>();
        _transformers = settings.Transformers?.ToDictionary(t => t.Name) ?? new Dictionary<string, Transformer>();
        _receivers = settings.Receivers?.ToDictionary(r => r.Name) ?? new Dictionary<string, Receiver>();

        // Создание связей для быстрого поиска по отношениям
        _trackingInstancesByConnection = settings.TrackingInstances.ToLookup(t => t.Connection);
        _filtersByTrackingInstance = (settings.Filters ?? []).ToLookup(f => f.TrackingInstance);
        _transformersByTrackingInstance = (settings.Transformers ?? [])
            .Where(t => !string.IsNullOrEmpty(t.TrackingInstance))
            .ToLookup(t => t.TrackingInstance!);
        _receiversByTrackingInstance = (settings.Receivers ?? []).ToLookup(r => r.TrackingInstance);
    }

    public Connection? GetConnection(string name)
    {
        return _connections.GetValueOrDefault(name);
    }

    public TrackingInstance? GetTrackingInstance(string name)
    {
        return _trackingInstances.GetValueOrDefault(name);
    }

    public Filter? GetFilter(string name)
    {
        return _filters.GetValueOrDefault(name);
    }

    public Transformer? GetTransformer(string name)
    {
        return _transformers.GetValueOrDefault(name);
    }

    public Receiver? GetReceiver(string name)
    {
        return _receivers.GetValueOrDefault(name);
    }

    public IReadOnlyList<TrackingInstance> GetTrackingInstancesForConnection(string connectionName)
    {
        return _trackingInstancesByConnection[connectionName].ToList();
    }

    public IReadOnlyList<Filter> GetFiltersForTrackingInstance(string trackingInstanceName)
    {
        return _filtersByTrackingInstance[trackingInstanceName].ToList();
    }

    public IReadOnlyList<Transformer> GetTransformersForTrackingInstance(string trackingInstanceName)
    {
        return _transformersByTrackingInstance[trackingInstanceName].ToList();
    }

    public IReadOnlyList<Receiver> GetReceiversForTrackingInstance(string trackingInstanceName)
    {
        return _receiversByTrackingInstance[trackingInstanceName].ToList();
    }

    public ReceiverPipeline GetReceiverPipeline(string receiverName)
    {
        var receiver = GetReceiver(receiverName);
        if (receiver == null) throw new NullReferenceException($"Receiver {receiverName} was not found");

        var trackingInstance = GetTrackingInstance(receiver.TrackingInstance);
        if (trackingInstance == null) throw new NullReferenceException($"Tracking instance {receiver.TrackingInstance} was not found");

        var connection = GetConnection(trackingInstance.Connection);
        if (connection == null) throw new NullReferenceException($"Connection {connection} was not found");

        Filter? filter = null;
        if (!string.IsNullOrEmpty(receiver.Filter))
        {
            filter = GetFilter(receiver.Filter);
        }

        Transformer? transformer = null;
        if (!string.IsNullOrEmpty(receiver.Transformer))
        {
            transformer = GetTransformer(receiver.Transformer);
        }

        return new ReceiverPipeline(receiver, trackingInstance, connection, filter, transformer);
    }

    public T? GetParameters<T>(string componentType, JsonElement parameters) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(parameters.GetRawText());
        }
        catch (JsonException)
        {
            return null;
        }
    }
}