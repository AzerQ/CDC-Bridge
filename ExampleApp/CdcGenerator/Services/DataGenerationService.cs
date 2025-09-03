using Bogus;
using CdcGenerator.Configuration;
using CdcGenerator.Models;
using Microsoft.Data.SqlClient;

namespace CdcGenerator.Services;

/// <summary>
/// Service for generating and manipulating data
/// </summary>
public class DataGenerationService
{
    private readonly AppSettings _settings;
    
    public DataGenerationService(AppSettings settings)
    {
        _settings = settings;
    }
    
    /// <summary>
    /// Generates fake data and inserts it into the table
    /// </summary>
    public async Task GenerateAndInsertDataAsync()
    {
        Console.WriteLine("Генерация и вставка данных...");
        
        // Создание генератора фейковых данных с помощью Bogus
        var faker = new Faker<Customer>("ru")
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName))
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.CreatedAt, f => f.Date.Recent());
        
        // Генерация фейковых клиентов
        var customers = faker.Generate(_settings.DataGenerationSettings.NumberOfCustomers);
        
        using (var connection = new SqlConnection(_settings.ConnectionStrings.DefaultConnection))
        {
            await connection.OpenAsync();
            
            foreach (var customer in customers)
            {
                var insertCommand = new SqlCommand(@"
                    INSERT INTO dbo.Customers (FirstName, LastName, Email, Phone, Address, CreatedAt)
                    VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @CreatedAt)", 
                    connection);
                
                insertCommand.Parameters.AddWithValue("@FirstName", customer.FirstName);
                insertCommand.Parameters.AddWithValue("@LastName", customer.LastName);
                insertCommand.Parameters.AddWithValue("@Email", customer.Email);
                insertCommand.Parameters.AddWithValue("@Phone", customer.Phone);
                insertCommand.Parameters.AddWithValue("@Address", customer.Address);
                insertCommand.Parameters.AddWithValue("@CreatedAt", customer.CreatedAt);
                
                await insertCommand.ExecuteNonQueryAsync();
                Console.WriteLine($"Добавлен клиент: {customer.FirstName} {customer.LastName}");
            }
        }
        
        Console.WriteLine("Данные успешно добавлены.");
        
        // Обновление нескольких записей для генерации событий CDC
        await UpdateSomeCustomersAsync();
        
        // Удаление одной записи для генерации события CDC
        await DeleteOneCustomerAsync();
    }
    
    /// <summary>
    /// Updates some customers to generate CDC events
    /// </summary>
    private async Task UpdateSomeCustomersAsync()
    {
        Console.WriteLine("Обновление некоторых записей...");
        
        using (var connection = new SqlConnection(_settings.ConnectionStrings.DefaultConnection))
        {
            await connection.OpenAsync();
            
            // Получение первых N клиентов для обновления
            var selectCommand = new SqlCommand(
                $"SELECT TOP {_settings.DataGenerationSettings.NumberOfCustomersToUpdate} Id FROM dbo.Customers ORDER BY Id", 
                connection);
            
            var customerIds = new List<int>();
            using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    customerIds.Add(reader.GetInt32(0));
                }
            }
            
            // Обновление каждого клиента
            foreach (var id in customerIds)
            {
                var faker = new Faker("ru");
                var newPhone = faker.Phone.PhoneNumber();
                var newAddress = faker.Address.FullAddress();
                
                var updateCommand = new SqlCommand(@"
                    UPDATE dbo.Customers 
                    SET Phone = @Phone, Address = @Address, UpdatedAt = GETDATE() 
                    WHERE Id = @Id", 
                    connection);
                
                updateCommand.Parameters.AddWithValue("@Phone", newPhone);
                updateCommand.Parameters.AddWithValue("@Address", newAddress);
                updateCommand.Parameters.AddWithValue("@Id", id);
                
                await updateCommand.ExecuteNonQueryAsync();
                Console.WriteLine($"Обновлен клиент с Id: {id}");
            }
        }
    }
    
    /// <summary>
    /// Deletes one customer to generate CDC event
    /// </summary>
    private async Task DeleteOneCustomerAsync()
    {
        Console.WriteLine("Удаление одной записи...");
        
        using (var connection = new SqlConnection(_settings.ConnectionStrings.DefaultConnection))
        {
            await connection.OpenAsync();
            
            // Получение последнего клиента для удаления
            var selectCommand = new SqlCommand(
                "SELECT TOP 1 Id FROM dbo.Customers ORDER BY Id DESC", 
                connection);
            
            var result = await selectCommand.ExecuteScalarAsync();
            if (result == null)
            {
                Console.WriteLine("Не найдено записей для удаления.");
                return;
            }
            
            var customerId = (int)result;
            
            // Удаление клиента
            var deleteCommand = new SqlCommand(
                "DELETE FROM dbo.Customers WHERE Id = @Id", 
                connection);
            
            deleteCommand.Parameters.AddWithValue("@Id", customerId);
            
            await deleteCommand.ExecuteNonQueryAsync();
            Console.WriteLine($"Удален клиент с Id: {customerId}");
        }
    }
}