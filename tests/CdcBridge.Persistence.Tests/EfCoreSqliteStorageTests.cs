using CdcBridge.Configuration;
using CdcBridge.Configuration.Models;
using CdcBridge.Core.Models;
using CdcBridge.Persistence.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CdcBridge.Persistence.Tests;

[TestClass]
public class EfCoreSqliteStorageTests
{
    private DbContextOptions<CdcBridgeDbContext> _dbContextOptions = null!;
    private Mock<ICdcConfigurationContext> _configContextMock = null!;
    private Mock<ILogger<EfCoreSqliteStorage>> _loggerMock = null!;
    private EfCoreSqliteStorage _storage = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<CdcBridgeDbContext>()
            .UseSqlite(_connection)
            .Options;

        using (var context = new CdcBridgeDbContext(_dbContextOptions))
        {
            context.Database.EnsureCreated();
        }

        _configContextMock = new Mock<ICdcConfigurationContext>();
        _loggerMock = new Mock<ILogger<EfCoreSqliteStorage>>();

        var dbContextFactoryMock = new Mock<IDbContextFactory<CdcBridgeDbContext>>();
        dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new CdcBridgeDbContext(_dbContextOptions));
        
        _storage = new EfCoreSqliteStorage(
            dbContextFactoryMock.Object,
            _configContextMock.Object,
            _loggerMock.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _connection.Close();
    }

    [TestMethod]
    public async Task GetLastProcessedRowLabelAsync_ShouldReturnNull_WhenStateDoesNotExist()
    {
        // Act
        var result = await _storage.GetLastProcessedRowLabelAsync("non-existent-instance");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SaveAndGetLastProcessedRowLabelAsync_ShouldReturnSavedLabel()
    {
        // Arrange
        var trackingInstanceName = "test-instance";
        var rowLabel = "test-label";

        // Act
        await _storage.SaveLastProcessedRowLabelAsync(trackingInstanceName, rowLabel);
        var result = await _storage.GetLastProcessedRowLabelAsync(trackingInstanceName);

        // Assert
        Assert.AreEqual(rowLabel, result);
    }

    [TestMethod]
    public async Task SaveLastProcessedRowLabelAsync_ShouldUpdateExistingLabel()
    {
        // Arrange
        var trackingInstanceName = "test-instance";
        var initialLabel = "initial-label";
        var updatedLabel = "updated-label";

        // Act
        await _storage.SaveLastProcessedRowLabelAsync(trackingInstanceName, initialLabel);
        await _storage.SaveLastProcessedRowLabelAsync(trackingInstanceName, updatedLabel);
        var result = await _storage.GetLastProcessedRowLabelAsync(trackingInstanceName);

        // Assert
        Assert.AreEqual(updatedLabel, result);
    }

    private JsonElement ToJsonElement(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    [TestMethod]
    public async Task AddChangesToBufferAsync_ShouldAddChanges()
    {
        // Arrange
        var trackingInstance = "test-tracking-instance";
        var changes = new List<TrackedChange>
        {
            new() { 
                TrackingInstance = trackingInstance, 
                ChangeType = ChangeType.Insert, 
                Data = new ChangeData { Old = null, New = ToJsonElement(new { id = 1, value = "new" }) }, 
                RowLabel = "lsn1" 
            },
            new() { 
                TrackingInstance = trackingInstance, 
                ChangeType = ChangeType.Update, 
                Data = new ChangeData { Old = ToJsonElement(new { id = 2 }), New = ToJsonElement(new { id = 2, value = "new2" }) }, 
                RowLabel = "lsn2" 
            }
        };
        
        var receivers = new List<Receiver>
        {
            new() { Name = "rec1", TrackingInstance = trackingInstance, Type = "test" },
            new() { Name = "rec2", TrackingInstance = trackingInstance, Type = "test" }
        }.AsReadOnly();

        _configContextMock.Setup(c => c.GetReceiversForTrackingInstance(trackingInstance))
            .Returns(receivers);

        // Act
        await _storage.AddChangesToBufferAsync(changes);

        // Assert
        using (var context = new CdcBridgeDbContext(_dbContextOptions))
        {
            var bufferedEvents = await context.BufferedChangeEvents.Include(e => e.DeliveryStatuses).ToListAsync();
            Assert.AreEqual(2, bufferedEvents.Count);

            var change1 = bufferedEvents.First(e => e.RowLabel == "lsn1");
            Assert.AreEqual(trackingInstance, change1.TrackingInstanceName);
            Assert.AreEqual(2, change1.DeliveryStatuses.Count);

            var change2 = bufferedEvents.First(e => e.RowLabel == "lsn2");
            Assert.AreEqual(trackingInstance, change2.TrackingInstanceName);
            Assert.AreEqual(2, change2.DeliveryStatuses.Count);
        }
    }

    [TestMethod]
    public async Task GetPendingChangesAsync_ShouldReturnCorrectChanges()
    {
        // Arrange
        var receiverName = "test-receiver";
        var trackingInstance = "test-tracking-instance";
        var changes = new List<TrackedChange>
        {
            new() { TrackingInstance = trackingInstance, RowLabel = "lsn1", ChangeType = ChangeType.Insert, Data = new ChangeData { New = ToJsonElement(new { id = 1 }) } },
            new() { TrackingInstance = trackingInstance, RowLabel = "lsn2", ChangeType = ChangeType.Insert, Data = new ChangeData { New = ToJsonElement(new { id = 2 }) } },
            new() { TrackingInstance = trackingInstance, RowLabel = "lsn3", ChangeType = ChangeType.Insert, Data = new ChangeData { New = ToJsonElement(new { id = 3 }) } }
        };
        
        var receivers = new List<Receiver> { new() { Name = receiverName, TrackingInstance = trackingInstance, Type = "test" } }.AsReadOnly();
        _configContextMock.Setup(c => c.GetReceiversForTrackingInstance(trackingInstance))
            .Returns(receivers);
        
        await _storage.AddChangesToBufferAsync(changes);

        // Mark lsn2 as processed (Success)
        using (var context = new CdcBridgeDbContext(_dbContextOptions))
        {
            var eventToMark = await context.BufferedChangeEvents.FirstAsync(e => e.RowLabel == "lsn2");
            await _storage.UpdateChangeStatusAsync(eventToMark.Id, trackingInstance, receiverName, true, null);
        }
        
        // Act
        var pendingChanges = await _storage.GetPendingChangesAsync(receiverName, trackingInstance, 10);

        // Assert
        var pendingChangesList = pendingChanges.ToList();
        Assert.AreEqual(2, pendingChangesList.Count);
        Assert.IsTrue(pendingChangesList.Any(c => c.RowLabel == "lsn1"));
        Assert.IsTrue(pendingChangesList.Any(c => c.RowLabel == "lsn3"));
        Assert.IsFalse(pendingChangesList.Any(c => c.RowLabel == "lsn2"));
    }

    [TestMethod]
    public async Task UpdateChangeStatusAsync_ShouldUpdateDeliveryStatus()
    {
        // Arrange
        var receiverName = "test-receiver";
        var trackingInstance = "test-tracking-instance";
        var changes = new List<TrackedChange>
        {
            new() { TrackingInstance = trackingInstance, RowLabel = "lsn1", ChangeType = ChangeType.Insert, Data = new ChangeData { New = ToJsonElement(new { id = 1 }) } },
            new() { TrackingInstance = trackingInstance, RowLabel = "lsn2", ChangeType = ChangeType.Insert, Data = new ChangeData { New = ToJsonElement(new { id = 2 }) } }
        };
        
        var receivers = new List<Receiver> { new() { Name = receiverName, TrackingInstance = trackingInstance, Type = "test" } }.AsReadOnly();
        _configContextMock.Setup(c => c.GetReceiversForTrackingInstance(trackingInstance))
            .Returns(receivers);
        
        await _storage.AddChangesToBufferAsync(changes);

        List<BufferedChangeEvent> eventsToMark;
        using (var context = new CdcBridgeDbContext(_dbContextOptions))
        {
            eventsToMark = await context.BufferedChangeEvents.ToListAsync();
        }

        // Act
        await _storage.UpdateChangeStatusAsync(eventsToMark[0].Id, trackingInstance, receiverName, true, null);
        await _storage.UpdateChangeStatusAsync(eventsToMark[1].Id, trackingInstance, receiverName, false, "Test Error");


        // Assert
        using (var context = new CdcBridgeDbContext(_dbContextOptions))
        {
            var statuses = await context.ReceiverDeliveryStatuses.ToListAsync();
            Assert.AreEqual(2, statuses.Count);

            var status1 = statuses.First(s => s.BufferedChangeEventId == eventsToMark[0].Id);
            Assert.AreEqual(DeliveryStatus.Success, status1.Status);
            Assert.AreEqual(1, status1.AttemptCount);
            Assert.IsNull(status1.ErrorDescription);

            var status2 = statuses.First(s => s.BufferedChangeEventId == eventsToMark[1].Id);
            Assert.AreEqual(DeliveryStatus.Failed, status2.Status);
            Assert.AreEqual(1, status2.AttemptCount);
            Assert.AreEqual("Test Error", status2.ErrorDescription);
        }
    }
}