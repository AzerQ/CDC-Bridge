using CdcBridge.Configuration;
using CdcBridge.Configuration.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CdcBridge.Configuration.Tests
{
    [TestClass]
    public class CdcConfigurationContextBuilderTests
    {
        private const string TestConfigDir = "TestConfigurations";
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void AddConfigurationFromFile_WithValidFile_BuildsContext()
        {
            // Arrange
            var builder = new CdcConfigurationContextBuilder();
            var filePath = Path.Combine(TestConfigDir, "valid_config.yaml");

            // Act
            var context = builder.AddConfigurationFromFile(filePath).Build();

            // Assert
            Assert.IsNotNull(context);
            Assert.AreEqual(1, context.CdcSettings.Connections.Count());
            Assert.AreEqual("TestConnection", context.CdcSettings.Connections.First().Name);
            Assert.AreEqual(1, context.CdcSettings.Receivers.Count());
            Assert.AreEqual("TestReceiver", context.CdcSettings.Receivers.First().Name);
            Assert.AreEqual(1, context.CdcSettings.TrackingInstances.Count());
            Assert.AreEqual("TestTrackingInstance", context.CdcSettings.TrackingInstances.First().Name);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void AddConfigurationFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var builder = new CdcConfigurationContextBuilder();
            var filePath = "non_existent_config.yaml";

            // Act
            builder.AddConfigurationFromFile(filePath);
        }

        [TestMethod]
        [ExpectedException(typeof(CdcConfigurationLoadException))]
        public void AddConfigurationFromFile_WithInvalidFile_ThrowsCdcConfigurationLoadException()
        {
            // Arrange
            var builder = new CdcConfigurationContextBuilder();
            var filePath = Path.Combine(TestConfigDir, "invalid_config_missing_connectionstring.yaml");

            // Act
            builder.AddConfigurationFromFile(filePath);
        }

        [TestMethod]
        [ExpectedException(typeof(CdcConfigurationLoadException))]
        public void AddConfiguration_WithInvalidConnection_ThrowsCdcConfigurationLoadException()
        {
            // Arrange
            var builder = new CdcConfigurationContextBuilder();
            var filePath = Path.Combine(TestConfigDir, "invalid_config_nonexistent_connection.yaml");

            // Act
            builder.AddConfigurationFromFile(filePath);
        }

        [TestMethod]
        public void AddConfigurationFromString_WithValidYaml_BuildsContext()
        {
            // Arrange
            var builder = new CdcConfigurationContextBuilder();
            var yamlContent = @"
connections:
  - name: ""StringConnection""
    type: ""mssql""
    connectionString: ""Server=.;""
receivers:
  - name: ""StringReceiver""
    type: ""webhook""
    trackingInstance: ""StringInstance""
    parameters: {}
trackingInstances:
  - name: ""StringInstance""
    connection: ""StringConnection""
    sourceTable: ""dbo.Orders""
    capturedColumns: [""Id""]
    checkIntervalInSeconds: 5
";

            // Act
            var context = builder.AddConfigurationFromString(yamlContent).Build();

            // Assert
            Assert.IsNotNull(context);
            Assert.AreEqual("StringConnection", context.CdcSettings.Connections.First().Name);
            Assert.AreEqual("StringInstance", context.CdcSettings.TrackingInstances.First().Name);
        }

        [TestMethod]
        public void AddConfiguration_MergesConfigurationsCorrectly()
        {
            // Arrange
            var builder = new CdcConfigurationContextBuilder();
            var settings1 = new CdcSettings
            {
                Connections = [new() { Name = "Conn1", Type = "mssql", ConnectionString = "CS1" }],
                Receivers = [new() { Name = "Rec1", Type = "webhook", TrackingInstance = "Inst1", Parameters = JsonSerializer.Deserialize<JsonElement>("{}") }],
                TrackingInstances = [new() { Name = "Inst1", Connection = "Conn1", SourceTable = "Table1", CapturedColumns = ["C1"], CheckIntervalInSeconds = 1}]
            };
            var settings2 = new CdcSettings
            {
                Connections = [new() { Name = "Conn2", Type = "mssql", ConnectionString = "CS2" }],
                TrackingInstances = [new() { Name = "Inst2", Connection = "Conn2", SourceTable = "Table2", CapturedColumns = ["C2"], CheckIntervalInSeconds = 2 }],
                Receivers = [new() { Name = "Rec2", Type = "webhook", TrackingInstance = "Inst2", Parameters = JsonSerializer.Deserialize<JsonElement>("{}") }]
            };

            // Act
            var context = builder.AddConfiguration(settings1).AddConfiguration(settings2).Build();

            // Assert
            Assert.AreEqual(2, context.CdcSettings.Connections.Count());
            Assert.AreEqual(2, context.CdcSettings.Receivers.Count());
            Assert.AreEqual(2, context.CdcSettings.TrackingInstances.Count());
            Assert.IsTrue(context.CdcSettings.Connections.Any(c => c.Name == "Conn1"));
            Assert.IsTrue(context.CdcSettings.Connections.Any(c => c.Name == "Conn2"));
        }
    }
}