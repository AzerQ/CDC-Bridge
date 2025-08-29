using Core;
using Microsoft.Data.Sqlite;
using Plugin.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using Xunit;

namespace Core.Tests
{
    public class EventLoggerTests
    {
        [Fact]
        public void LogEvent_InsertsEventIntoDatabase()
        {
            // Arrange
            var dbPath = ":memory:";
            var logger = new EventLogger(dbPath);
            var change = new ChangeEvent { 
                Table = "TestTable", 
                Type = ChangeType.Insert, 
                NewData = JsonSerializer.Serialize(new Dictionary<string, object> { { "Id", 1 } }) 
            };

            // Act
            logger.LogEvent(change);

            // Assert
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Events";
            var count = (long)command.ExecuteScalar();
            Assert.Equal(1, count);
        }
    }
}