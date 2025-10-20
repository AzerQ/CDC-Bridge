using CdcBridge.Configuration.Models;
using CdcBridge.Configuration.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CdcBridge.Configuration.Tests;

[TestClass]
public class TrackingInstanceValidatorTests
{
    private TrackingInstanceValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new TrackingInstanceValidator();
    }

    [TestMethod]
    public void Validate_WithValidTrackingInstance_Succeeds()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id", "Name" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };

        // Act
        var result = _validator.Validate(trackingInstance);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_WithEmptyName_Fails()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };

        // Act
        var result = _validator.Validate(trackingInstance);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Name"));
    }

    [TestMethod]
    public void Validate_WithEmptySourceTable_Fails()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };

        // Act
        var result = _validator.Validate(trackingInstance);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "SourceTable"));
    }

    [TestMethod]
    public void Validate_WithEmptyCapturedColumns_Fails()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string>(),
            Connection = "TestConnection",
            CheckIntervalInSeconds = 60
        };

        // Act
        var result = _validator.Validate(trackingInstance);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "CapturedColumns"));
    }

    [TestMethod]
    public void Validate_WithNegativeCheckInterval_Fails()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = -1
        };

        // Act
        var result = _validator.Validate(trackingInstance);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "CheckIntervalInSeconds"));
    }

    [TestMethod]
    public void Validate_WithZeroCheckInterval_Fails()
    {
        // Arrange
        var trackingInstance = new TrackingInstance
        {
            Name = "TestInstance",
            SourceTable = "dbo.Orders",
            CapturedColumns = new List<string> { "Id" },
            Connection = "TestConnection",
            CheckIntervalInSeconds = 0
        };

        // Act
        var result = _validator.Validate(trackingInstance);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "CheckIntervalInSeconds"));
    }
}
