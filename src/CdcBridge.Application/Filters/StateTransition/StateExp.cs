namespace CdcBridge.Application.Filters.StateTransition;

public class StateExp
{
    private const string AnyStateSymbol = "*";
    public string Value { get; }
    public bool IsAnyState => Value.Equals(AnyStateSymbol);
	
    public StateExp(string stateValue) {
        Value = stateValue;
    }
	
    public bool AnotherStateApproach(StateExp otherState, bool ignoreCase = true) {
        return this.IsAnyState ||
               otherState.IsAnyState ||
               this.Value.Equals(otherState.Value, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
    }
	
}