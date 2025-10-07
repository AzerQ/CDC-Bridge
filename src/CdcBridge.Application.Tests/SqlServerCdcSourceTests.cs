using System.Numerics;
using System.Text.Json;
using CdcBridge.Application.CdcSources;
using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MsSqlCdc;

namespace CdcBridge.Application.Tests;

[TestClass]
public class SqlServerCdcSourceTests
{
    private Mock<IMsSqlChangesProvider> _mockProvider = null!;
    private SqlServerCdcSource _cdcSource = null!;
    private Connection _connection = null!;
    private TrackingInstance _trackingInstance = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockProvider = new Mock<IMsSqlChangesProvider>();
        
        // Устанавливаем фабрику для создания нашего mock-провайдера
        SqlServerCdcSource.ProviderFactory = _ => _mockProvider.Object;

        _cdcSource = new SqlServerCdcSource();
        _connection = new Connection { Name = "TestConnection", Type = "SqlServer", ConnectionString = "Server=test;Database=test;User Id=test;Password=test;" };
        _trackingInstance = new TrackingInstance { Name = "TestInstance", Connection = "TestConnection", SourceTable = "TestTable", SourceSchema = "dbo" };
    }

    [TestMethod]
    public async Task GetChanges_ShouldReturnTrackedChanges_WhenChangesAreAvailable()
    {
        // Arrange
        var changedRows = new List<AllChangeRow>
        {
            new(BigInteger.One, BigInteger.One, AllChangeOperation.Insert, Array.Empty<byte>(), "__$start_lsn", new Dictionary<string, object> { { "id", 1 }, { "value", "test" } })
        };

        var trackedChanges = new List<TrackedChange>
        {
            new()
            {
                TrackingInstance = "TestInstance",
                RowLabel = "1",
                ChangeType = ChangeType.Insert,
                Data = new ChangeData { New = JsonSerializer.SerializeToElement(new { id = 1, value = "test" }) }
            }
        };

        _mockProvider.Setup(p => p.GetChangedRows(_trackingInstance, null)).ReturnsAsync(changedRows);
        _mockProvider.Setup(p => p.MapChangeRowsToTrackedChanges(changedRows, _trackingInstance)).ReturnsAsync(trackedChanges);

        var trackingInstanceInfo = new TrackingInstanceInfo(_trackingInstance, _connection);

        // Act
        var result = await _cdcSource.GetChanges(trackingInstanceInfo);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(ChangeType.Insert, result.First().ChangeType);
        _mockProvider.Verify(p => p.GetChangedRows(_trackingInstance, null), Times.Once);
        _mockProvider.Verify(p => p.MapChangeRowsToTrackedChanges(changedRows, _trackingInstance), Times.Once);
    }

    [TestMethod]
    public async Task CheckCdcIsEnabled_ShouldReturnProviderResult()
    {
        // Arrange
        var expectedResult = (isEnabled: true, message: "Enabled", dbTrackingInstanceName: "dbo_TestTable");
        _mockProvider.Setup(p => p.CheckCdcIsEnabled(_trackingInstance)).ReturnsAsync(expectedResult);

        var trackingInstanceInfo = new TrackingInstanceInfo(_trackingInstance, _connection);

        // Act
        var result = await _cdcSource.CheckCdcIsEnabled(trackingInstanceInfo);

        // Assert
        Assert.AreEqual(expectedResult, result);
        _mockProvider.Verify(p => p.CheckCdcIsEnabled(_trackingInstance), Times.Once);
    }

    [TestMethod]
    public async Task GetChanges_ShouldReturnEmptyCollection_WhenNoChangesAreAvailable()
    {
        // Arrange
        var emptyChangedRows = new List<AllChangeRow>();
        var emptyTrackedChanges = new List<TrackedChange>();

        _mockProvider.Setup(p => p.GetChangedRows(_trackingInstance, null)).ReturnsAsync(emptyChangedRows);
        _mockProvider.Setup(p => p.MapChangeRowsToTrackedChanges(emptyChangedRows, _trackingInstance)).ReturnsAsync(emptyTrackedChanges);

        var trackingInstanceInfo = new TrackingInstanceInfo(_trackingInstance, _connection);

        // Act
        var result = await _cdcSource.GetChanges(trackingInstanceInfo);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Any());
        _mockProvider.Verify(p => p.GetChangedRows(_trackingInstance, null), Times.Once);
        _mockProvider.Verify(p => p.MapChangeRowsToTrackedChanges(emptyChangedRows, _trackingInstance), Times.Once);
    }
}
