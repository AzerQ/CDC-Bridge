namespace CdcBridge.Application.Filters.StateTransition;

public class StateTransitionChecker {

	private readonly List<StateTransition> _allowedStatesTransitions;
	
	private readonly bool _allowTransitionToTheSameState;
	
 	public bool TransitionAllowed(string startStateParam, string finalStateParam)
    {
	    bool isStateEquals =
		    string.Equals(startStateParam, finalStateParam, StringComparison.InvariantCultureIgnoreCase);
	    
	    if (!_allowTransitionToTheSameState && isStateEquals) 
		    return false;
	    
		return _allowedStatesTransitions
			   .Any(t => t.TransitionAllowed(startStateParam, finalStateParam));
	}
	
	const string StateTransitionSeparator = ";";
	private IEnumerable<StateTransition> ParseStateTransitions(string expression) {
		
		var transitions = 
		expression
		.Split(StateTransitionSeparator)
		.Select(s => s.Trim())
		.Where(s => !string.IsNullOrWhiteSpace(s) && s != Environment.NewLine)
		.Select(StateTransition.Parse);
		
		return transitions;
	}
	
	public StateTransitionChecker(string transitionsExpression, bool allowTransitionToTheSameState = false) {
		_allowedStatesTransitions = ParseStateTransitions(transitionsExpression).ToList();
		_allowTransitionToTheSameState = allowTransitionToTheSameState;
	}
}