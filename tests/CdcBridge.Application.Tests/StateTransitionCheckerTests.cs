using System.Data;
using CdcBridge.Application.Filters.StateTransition;

namespace CdcBridge.Application.Tests;

[TestClass]
public class StateTransitionCheckerTests
{

    [DataRow("State2 => State3", "State2", "State3", true)]
    [DataRow("* => State4", "State2", "State4", true)]
    [DataRow("State10 => State4", "State3", "State6", false)]
    [DataRow("State3 => *", "State3", "State10", true)]
    [DataRow("* => State5", "State1", "State6", false)]
    [DataRow("State3 => *", "State6", "State10", false)]
    [TestMethod]
    public void TestSingleExpressionsAllowed(string expression, string startState, string endState, bool expectedResult)
    {
        var transitionChecker = new StateTransitionChecker(expression);
        bool actualResult = transitionChecker.TransitionAllowed(startState, endState);
        Assert.AreEqual(expectedResult, actualResult);
    }

    [DataRow("State2 => State3; State5 => State10", "State5", "State10", true)]
    [DataRow("* => State4; State3 => *", "State3", "State4", true)]
    [DataRow("State10 => State4; * => State5", "State7", "State6", false)]
    [DataRow("State3 => *; State6 => State8", "State6", "State10", false)]
    [TestMethod]
    public void TestMultipleExpressionsAllowed(string expression, string startState, string endState,
        bool expectedResult)
    {
        var transitionChecker = new StateTransitionChecker(expression);
        bool actualResult = transitionChecker.TransitionAllowed(startState, endState);
        Assert.AreEqual(expectedResult, actualResult);
    }

    [DataRow("State2 => ")]
    [DataRow(" => ")]
    [DataRow(" => State4")]
    [DataRow("State3 | *")]
    [DataRow("Abracadabra")] 
    [TestMethod]
    public void TestIncorrectExpressionsThrowsException(string expression)
    {
        Assert.ThrowsException<InvalidExpressionException>(() => new StateTransitionChecker(expression));
    }
    
}