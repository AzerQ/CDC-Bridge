using System.Data;

namespace CdcBridge.Application.Filters.StateTransition;

public class StateTransition
{
    private const string StatesSeparatorSymbol = "=>";
   
    public required StateExp StartState { get; set; }
    public required StateExp FinalState { get; set; }


    public static StateTransition Parse(string stateExpression)
    {

        var statesParts = stateExpression
            .Split(StatesSeparatorSymbol)
            .Select(s => s.Trim())
            .ToArray();

        var startStateStr = statesParts.ElementAtOrDefault(0);
        var finalStateStr = statesParts.ElementAtOrDefault(1);

        if (string.IsNullOrEmpty(startStateStr) ||
            string.IsNullOrEmpty(finalStateStr) )
        {
            throw new InvalidExpressionException("Не заданно начальное или конечное состояние в выражении [StartState] => [FinalState]");
        }
		
        return new StateTransition {
            StartState = new StateExp(startStateStr),
            FinalState = new StateExp(finalStateStr)
        };
    }
	
    public bool TransitionAllowed(string startStateParam, string finalStateParam) {
		
        var startState = new StateExp(startStateParam);
        var finalState = new StateExp(finalStateParam);
		
        return startState.AnotherStateApproach(this.StartState)
               &&
               finalState.AnotherStateApproach(this.FinalState);
		
    }

}