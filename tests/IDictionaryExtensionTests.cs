using CdcBridge.Application.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.Json;

namespace CdcBridge.Application.Tests;

[TestClass]
public class IDictionaryExtensionTests
{
    [TestMethod]
    public void ToJsonElement_WithSimpleDictionary_ReturnsJsonElement()
    {
        // Arrange
        IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>
        {
            { "id", 1 },
            { "name", "Test" },
            { "isActive", true }
        };

        // Act
        var result = dictionary.ToJsonElement();

        // Assert
        Assert.AreEqual(JsonValueKind.Object, result.ValueKind);
        Assert.AreEqual(1, result.GetProperty("id").GetInt32());
        Assert.AreEqual("Test", result.GetProperty("name").GetString());
        Assert.IsTrue(result.GetProperty("isActive").GetBoolean());
    }

    [TestMethod]
    public void ToJsonElement_WithNestedDictionary_ReturnsNestedJsonElement()
    {
        // Arrange
        IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>
        {
            { "id", 1 },
            { "nested", new Dictionary<string, object>
                {
                    { "value", "nested-value" }
                }
            }
        };

        // Act
        var result = dictionary.ToJsonElement();

        // Assert
        Assert.AreEqual(JsonValueKind.Object, result.ValueKind);
        Assert.AreEqual(1, result.GetProperty("id").GetInt32());
        Assert.AreEqual("nested-value", result.GetProperty("nested").GetProperty("value").GetString());
    }

    [TestMethod]
    public void ToJsonElement_WithEmptyDictionary_ReturnsEmptyJsonObject()
    {
        // Arrange
        IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>();

        // Act
        var result = dictionary.ToJsonElement();

        // Assert
        Assert.AreEqual(JsonValueKind.Object, result.ValueKind);
        Assert.AreEqual(0, result.EnumerateObject().Count());
    }
}
