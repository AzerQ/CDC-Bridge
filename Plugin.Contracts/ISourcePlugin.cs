using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Contracts;

/// <summary>
/// Interface for source plugins that retrieve change events.
/// </summary>
public interface ISourcePlugin
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Retrieves change events asynchronously.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the collection of change events.</returns>
    Task<IEnumerable<ChangeEvent>> GetChangesAsync(CancellationToken token);
}