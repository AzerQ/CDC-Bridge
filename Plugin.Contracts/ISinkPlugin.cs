using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Contracts;

/// <summary>
/// Represents the result of a delivery operation.
/// </summary>
public class DeliveryResult
{
    /// <summary>
    /// Indicates whether the delivery was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the delivery failed.
    /// </summary>
    public string ErrorMessage { get; set; }
}

/// <summary>
/// Interface for sink plugins that send change events.
/// </summary>
public interface ISinkPlugin
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sends a change event asynchronously.
    /// </summary>
    /// <param name="change">The change event to send.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the delivery result.</returns>
    Task<DeliveryResult> SendAsync(ChangeEvent change, CancellationToken token);
}