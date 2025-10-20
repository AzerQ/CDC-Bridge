using CdcBridge.Application.Filters;
using CdcBridge.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace CdcBridge.Application.Tests;

[TestClass]
public class JsonPathFilterTests
{
    private TrackedChange CreateTrackedChange(object? newData, object? oldData = null)
    {
        return new TrackedChange
        {
            TrackingInstance = "test-instance",
            ChangeType = ChangeType.Insert,
            CreatedAt = System.DateTime.UtcNow,
            RowLabel = "test-row",
            Data = new ChangeData
            {
                New = newData != null ? JsonSerializer.SerializeToElement(newData) : (JsonElement?)null,
                Old = oldData != null ? JsonSerializer.SerializeToElement(oldData) : (JsonElement?)null
            }
        };
    }

    private JsonElement CreateParameters(string expression)
    {
        var json = $"{{\"expression\": \"{expression}\"}}";
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    [TestMethod]
    public void IsMatch_WithMatchingExpression_ReturnsTrue()
    {
        // Arrange
        var filter = new JsonPathFilter();
        var trackedChange = CreateTrackedChange(new { status = "active" });
        var parameters = CreateParameters("$[?(@.data.new.status == 'active')]");

        // Act
        var result = filter.IsMatch(trackedChange, parameters);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsMatch_WithNonMatchingExpression_ReturnsFalse()
    {
        // Arrange
        var filter = new JsonPathFilter();
        var trackedChange = CreateTrackedChange(new { status = "inactive" });
        var parameters = CreateParameters("$[?(@.data.new.status == 'active')]");

        // Act
        var result = filter.IsMatch(trackedChange, parameters);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void IsMatch_WithNullChange_ThrowsArgumentNullException()
    {
        // Arrange
        var filter = new JsonPathFilter();
        var parameters = CreateParameters("$");

        // Act
        filter.IsMatch(null!, parameters);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void IsMatch_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var filter = new JsonPathFilter();
        var trackedChange = CreateTrackedChange(new { });

        // Act
        filter.IsMatch(trackedChange, default);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void IsMatch_WithMissingExpression_ThrowsInvalidOperationException()
    {
        // Arrange
        var filter = new JsonPathFilter();
        var trackedChange = CreateTrackedChange(new { });
        var parameters = JsonSerializer.Deserialize<JsonElement>(@"{""other_param"": ""value""}");

        // Act
        filter.IsMatch(trackedChange, parameters);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void IsMatch_WithInvalidExpression_ThrowsInvalidOperationException()
    {
        // Arrange
        var filter = new JsonPathFilter();
        var trackedChange = CreateTrackedChange(new { id = 1, status = "active" });
        // Это выражение имеет незакрытую скобку, что делает его невалидным.
        var parameters = CreateParameters("$[?(@.id == 1)");

        // Act
        filter.IsMatch(trackedChange, parameters);
    }
}
