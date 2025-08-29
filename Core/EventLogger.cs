// Название задачи: Настройка журнала событий в SQLite
// Описание задачи: Класс для логирования событий изменений в базу данных SQLite.
// Чек-лист выполнения задачи:
// - [x] Создание базы данных и таблицы
// - [x] Метод для логирования события
// - [x] Интеграция в CoreService
// - [x] Документация
// - [x] Unit-тесты

using Microsoft.Data.Sqlite;
using Plugin.Contracts;
using System;
using System.Text.Json;

namespace Core;

public class EventLogger
{
    private readonly string _connectionString;

    public EventLogger(string dbPath = "events.db")
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Events (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                TableName TEXT,
                ChangeType TEXT,
                Data TEXT
            );
        ";
        command.ExecuteNonQuery();
    }

    public void LogEvent(ChangeEvent changeEvent)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Events (TableName, ChangeType, Data)
            VALUES (@TableName, @ChangeType, @Data);
        ";
        command.Parameters.AddWithValue("@TableName", changeEvent.Table);
        command.Parameters.AddWithValue("@ChangeType", changeEvent.Type.ToString());
        command.Parameters.AddWithValue("@Data", JsonSerializer.Serialize(new { changeEvent.OldData, changeEvent.NewData }));
        command.ExecuteNonQuery();
    }
}

// Documentation:
// EventLogger class provides logging of ChangeEvent instances to a SQLite database.
// Usage example:
// var logger = new EventLogger("path/to/db");
// logger.LogEvent(new ChangeEvent { Table = "Users", Type = ChangeType.Insert, NewData = "New user data" });