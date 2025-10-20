using CdcBridge.Application.DI;
using CdcBridge.Example.WorkerService.services;
using CdcBridge.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<DataGenerator>();
builder.Services.AddHostedService<Producer>();
builder.Services.AddCdcBridge(builder.Configuration);

var host = builder.Build();

// Автоматическое применение миграций при старте
// using (var scope = host.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<CdcBridgeDbContext>();
//     dbContext.Database.Migrate();
// }


host.Run();
