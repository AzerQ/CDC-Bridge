using CdcGenerator.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace CdcGenerator.Services
{
    public class DatabaseService
    {
        private readonly AppSettings _settings;

        public DatabaseService(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task CreateDatabaseAndTableAsync()
        {
            // Подключение к мастер-базе для создания базы данных
            using var connection = new SqlConnection(_settings.ConnectionStrings.MasterConnection);
            await connection.OpenAsync();

            Console.WriteLine("Подключение к SQL Server установлено.");

            // Проверка существования базы данных
            var checkDbCommand = new SqlCommand(
                "SELECT COUNT(*) FROM sys.databases WHERE name = 'CdcExampleDb'",
                connection);

            var result = await checkDbCommand.ExecuteScalarAsync();
            var dbExists = result != null && (int)result > 0;

            if (!dbExists)
            {
                Console.WriteLine("Создание базы данных...");
                var createDbCommand = new SqlCommand(
                    "CREATE DATABASE CdcExampleDb",
                    connection);

                await createDbCommand.ExecuteNonQueryAsync();
                Console.WriteLine("База данных создана.");
            }
            else
            {
                Console.WriteLine("База данных уже существует.");
            }
        }

        public async Task CreateTableAsync()
        {
            // Подключение к созданной базе данных
            using var connection = new SqlConnection(_settings.ConnectionStrings.DefaultConnection);
            await connection.OpenAsync();

            // Проверка существования таблицы
            var checkTableCommand = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Customers'",
                connection);

            var tableResult = await checkTableCommand.ExecuteScalarAsync();
            var tableExists = tableResult != null && (int)tableResult > 0;

            if (!tableExists)
            {
                Console.WriteLine("Создание таблицы...");
                var createTableCommand = new SqlCommand(
                    @"
                    CREATE TABLE dbo.Customers (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FirstName NVARCHAR(50) NOT NULL,
                        LastName NVARCHAR(50) NOT NULL,
                        Email NVARCHAR(100) NOT NULL,
                        Phone NVARCHAR(20) NULL,
                        Address NVARCHAR(200) NULL,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NULL
                    )
                    ",
                    connection);

                await createTableCommand.ExecuteNonQueryAsync();
                Console.WriteLine("Таблица создана.");
            }
            else
            {
                Console.WriteLine("Таблица уже существует.");
            }
        }

        public async Task EnableCdcAsync()
        {
            // Подключение к созданной базе данных
            using var connection = new SqlConnection(_settings.ConnectionStrings.DefaultConnection);
            await connection.OpenAsync();

            // Проверка, включен ли CDC для базы данных
            var checkDbCdcCommand = new SqlCommand(
                "SELECT COUNT(*) FROM sys.databases WHERE name = 'CdcExampleDb' AND is_cdc_enabled = 1",
                connection);

            var dbCdcResult = await checkDbCdcCommand.ExecuteScalarAsync();
            var dbCdcEnabled = dbCdcResult != null && (int)dbCdcResult > 0;

            if (!dbCdcEnabled)
            {
                Console.WriteLine("Включение CDC для базы данных...");
                // Попытка изменить владельца базы данных (может не сработать, если нет прав)
                var changeOwnerCommand = new SqlCommand("EXEC sp_changedbowner 'sa'", connection);
                await changeOwnerCommand.ExecuteNonQueryAsync();
                Console.WriteLine("Владелец базы данных изменен на 'sa'.");

                // Затем включим CDC для базы данных
                var enableDbCdcCommand = new SqlCommand("EXEC sys.sp_cdc_enable_db", connection);
                await enableDbCdcCommand.ExecuteNonQueryAsync();
                Console.WriteLine("CDC для базы данных включен.");
            }
            else
            {
                Console.WriteLine("CDC для базы данных уже включен.");
            }

            // Проверка, включен ли CDC для таблицы
            var checkTableCdcCommand = new SqlCommand(
                "SELECT COUNT(*) FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name + '.' + t.name = @tableName AND t.is_tracked_by_cdc = 1",
                connection);

            checkTableCdcCommand.Parameters.AddWithValue("@tableName", _settings.DatabaseSettings.TableName);

            var tableCdcResult = await checkTableCdcCommand.ExecuteScalarAsync();
            var tableCdcEnabled = tableCdcResult != null && (int)tableCdcResult > 0;

            if (!tableCdcEnabled)
            {
                Console.WriteLine("Включение CDC для таблицы...");
                var enableTableCdcCommand = new SqlCommand(
                    $"EXEC sys.sp_cdc_enable_table @source_schema = 'dbo', @source_name = 'Customers', @role_name = NULL, @capture_instance = '{_settings.DatabaseSettings.CaptureInstance}'",
                    connection);

                await enableTableCdcCommand.ExecuteNonQueryAsync();
                Console.WriteLine("CDC для таблицы включен.");
            }
            else
            {
                Console.WriteLine("CDC для таблицы уже включен.");
            }
        }
    }
}