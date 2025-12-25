using System.Text.Json;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CdcBridge.Application.Filters.StateTransition;

public class StateTransitionFilter(IMemoryCache cache) : IFilter
{
    public string Name => nameof(StateTransitionFilter);
    public bool IsMatch(TrackedChange trackedChange, JsonElement parameters)
    {
        var filterParams = parameters.Deserialize<StateTransitionFilterParams>();
        
        if (filterParams == null)
            throw new ArgumentException("Cannot parse parameters from json. Result type (StateTransitionFilterParams)");
        
        if (trackedChange.ChangeType != ChangeType.Update)
            return false;

        var oldState = trackedChange.Data.Old?.GetProperty(filterParams.Column).GetString();
        var newState = trackedChange.Data.New?.GetProperty(filterParams.Column).GetString();
        
        if (oldState is null || newState is null)
            return false;

        var stateTransitionChecker =
            cache.GetOrCreate(filterParams.Expression, entry => new StateTransitionChecker((string)entry.Key))!;
        
        bool isMatch = stateTransitionChecker.TransitionAllowed(oldState, newState);
        return isMatch;
    }
}

public class StateTransitionFilterParams
{
    public required string Column { get; set; }

    public required string Expression { get; set; }
    
}