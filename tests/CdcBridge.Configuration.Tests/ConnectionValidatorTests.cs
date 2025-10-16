using CdcBridge.Configuration.Models;
using CdcBridge.Configuration.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CdcBridge.Configuration.Tests;

[TestClass]
public class ConnectionValidatorTests
{
    private ConnectionValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new ConnectionValidator();
    }

    [TestMethod]
    public void Validate_WithValidConnection_Succeeds()
    {
        // Arrange
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;Database=test;",
            Active = true
        };

        // Act
        var result = _validator.Validate(connection);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_WithEmptyName_Fails()
    {
        // Arrange
        var connection = new Connection
        {
            Name = "",
            Type = "SqlServer",
            ConnectionString = "Server=localhost;Database=test;",
            Active = true
        };

        // Act
        var result = _validator.Validate(connection);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Name"));
    }

    [TestMethod]
    public void Validate_WithEmptyType_Fails()
    {
        // Arrange
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "",
            ConnectionString = "Server=localhost;Database=test;",
            Active = true
        };

        // Act
        var result = _validator.Validate(connection);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "Type"));
    }

    [TestMethod]
    public void Validate_WithEmptyConnectionString_Fails()
    {
        // Arrange
        var connection = new Connection
        {
            Name = "TestConnection",
            Type = "SqlServer",
            ConnectionString = "",
            Active = true
        };

        // Act
        var result = _validator.Validate(connection);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "ConnectionString"));
    }
}
