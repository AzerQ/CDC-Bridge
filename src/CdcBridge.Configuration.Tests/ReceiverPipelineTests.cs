using CdcBridge.Configuration;
using CdcBridge.Configuration.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace CdcBridge.Configuration.Tests;

[TestClass]
public class ReceiverPipelineTests
{
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var receiver = new Receiver
        {
            Name = "TestReceiver",
            Type = "webhook",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;",
            Active = true
        };

        // Act
        var pipeline = new ReceiverPipeline(receiver, trackingInstance, connection, null, null);

        // Assert
        Assert.IsNotNull(pipeline);
        Assert.AreEqual(receiver, pipeline.Receiver);
        Assert.AreEqual(trackingInstance, pipeline.TrackingInstance);
        Assert.AreEqual(connection, pipeline.Connection);
        Assert.IsNull(pipeline.Filter);
        Assert.IsNull(pipeline.Transformer);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullReceiver_ThrowsArgumentNullException()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;",
            Active = true
        };

        // Act
        var pipeline = new ReceiverPipeline(null!, trackingInstance, connection, null, null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullTrackingInstance_ThrowsArgumentNullException()
    {
        // Arrange
        var receiver = new Receiver
        {
            Name = "TestReceiver",
            Type = "webhook",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;",
            Active = true
        };

        // Act
        var pipeline = new ReceiverPipeline(receiver, null!, connection, null, null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullConnection_ThrowsArgumentNullException()
    {
        // Arrange
        var receiver = new Receiver
        {
            Name = "TestReceiver",
            Type = "webhook",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };

        // Act
        var pipeline = new ReceiverPipeline(receiver, trackingInstance, null!, null, null);
    }

    [TestMethod]
    public void ToString_WithFilterAndTransformer_ReturnsCompleteString()
    {
        // Arrange
        var receiver = new Receiver
        {
            Name = "TestReceiver",
            Type = "webhook",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;",
            Active = true
        };
        var filter = new Filter
        {
            Name = "TestFilter",
            Type = "jsonpath",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };
        var transformer = new Transformer
        {
            Name = "TestTransformer",
            Type = "jsonata",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };

        // Act
        var pipeline = new ReceiverPipeline(receiver, trackingInstance, connection, filter, transformer);
        var result = pipeline.ToString();

        // Assert
        Assert.IsTrue(result.Contains("dbo.Orders"));
        Assert.IsTrue(result.Contains("TestFilter"));
        Assert.IsTrue(result.Contains("TestTransformer"));
        Assert.IsTrue(result.Contains("TestReceiver"));
    }

    [TestMethod]
    public void ToString_WithoutFilterAndTransformer_ReturnsStringWithNoFilter()
    {
        // Arrange
        var receiver = new Receiver
        {
            Name = "TestReceiver",
            Type = "webhook",
            TrackingInstance = "TestInstance",
            Parameters = JsonSerializer.Deserialize<JsonElement>("{}")
        };
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;",
            Active = true
        };

        // Act
        var pipeline = new ReceiverPipeline(receiver, trackingInstance, connection, null, null);
        var result = pipeline.ToString();

        // Assert
        Assert.IsTrue(result.Contains("dbo.Orders"));
        Assert.IsTrue(result.Contains("No Filter"));
        Assert.IsTrue(result.Contains("No Transformer"));
        Assert.IsTrue(result.Contains("TestReceiver"));
    }
}
