using CdcBridge.Application.Transformers;
using CdcBridge.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace CdcBridge.Application.Tests;

[TestClass]
public class JSONataTransformerTests
{
    private TrackedChange CreateTrackedChange(object newData)
    {
        return new TrackedChange
        {
            TrackingInstance = "test-instance",
            ChangeType = ChangeType.Insert,
            CreatedAt = DateTime.UtcNow,
            RowLabel = "test-row",
            Data = new ChangeData
            {
                New = JsonSerializer.SerializeToElement(newData)
            }
        };
    }

    private JsonElement CreateParameters(string expression)
    {
        var json = $"{{\"expression\": {JsonSerializer.Serialize(expression)}}}";
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Transform_WithNullChange_ThrowsArgumentNullException()
    {
        // Arrange
        var transformer = new JSONataTransformer();
        var parameters = CreateParameters("data.new");

        // Act
        transformer.Transform(null!, parameters);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Transform_WithInvalidExpression_ThrowsInvalidOperationException()
    {
        // Arrange
        var transformer = new JSONataTransformer();
        var trackedChange = CreateTrackedChange(new { id = 1 });
        // Invalid JSONata expression with unclosed bracket
        var parameters = CreateParameters("{\"id\": data.new.id");

        // Act
        transformer.Transform(trackedChange, parameters);
    }

    [TestMethod]
    public void Name_ReturnsCorrectName()
    {
        // Arrange
        var transformer = new JSONataTransformer();

        // Act & Assert
        Assert.AreEqual("JSONataTransformer", transformer.Name);
    }
}

