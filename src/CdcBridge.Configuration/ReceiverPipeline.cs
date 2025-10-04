using CdcBridge.Configuration.Models;

namespace CdcBridge.Configuration;

public class ReceiverPipeline(
    Receiver receiver,
    TrackingInstance trackingInstance,
    Connection connection,
    Filter? filter,
    Transformer? transformer)
{
    public Receiver Receiver { get; } = receiver ?? throw new ArgumentNullException(nameof(receiver));
    public TrackingInstance TrackingInstance { get; } = trackingInstance ?? throw new ArgumentNullException(nameof(trackingInstance));
    public Connection Connection { get; } = connection ?? throw new ArgumentNullException(nameof(connection));
    public Filter? Filter { get; } = filter;
    public Transformer? Transformer { get; } = transformer;

    public override string ToString()
    {
        return $"{TrackingInstance.SourceTable} -> {Filter?.Name ?? "No Filter"} -> {Transformer?.Name ?? "No Transformer"} -> {Receiver.Name}";
    }
}