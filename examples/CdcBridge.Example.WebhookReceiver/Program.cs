using System.Text.Json;
using CdcBridge.Example.WebhookReceiver;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// Добавляем наш сервис для хранения событий в DI как Singleton
builder.Services.AddSingleton<InMemoryChangeEventStore>();

var app = builder.Build();

app.UseHttpsRedirection();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// --- Эндпоинт для ПРИЕМА изменений ---
// Он будет принимать POST запросы на /webhooks/{entityName}
// Например: /webhooks/employee, /webhooks/department
app.MapPost("/webhooks/{entityName}", (
    [FromRoute] string entityName, 
    [FromBody] JsonElement changeEvent, // Принимаем любой JSON
    [FromServices] InMemoryChangeEventStore store) =>
{
    logger.LogInformation("Received change for entity '{EntityName}'. Event Body: {Body}", 
        entityName, 
        changeEvent.ToString());
    
    // Сохраняем полученные данные в памяти
    store.AddChange(entityName, changeEvent);

    return Results.Ok(new { message = $"Event for {entityName} received." });
});


// --- Эндпоинт для ПРОВЕРКИ поступивших изменений ---
// Он будет отдавать все, что накопилось в памяти, по GET запросу на /changes
app.MapGet("/changes", ([FromServices] InMemoryChangeEventStore store) =>
{
    logger.LogInformation("Fetching all stored changes.");
    
    var allChanges = store.GetAllChanges();
    
    // Возвращаем все сохраненные события
    return Results.Ok(allChanges);
});

app.Run();