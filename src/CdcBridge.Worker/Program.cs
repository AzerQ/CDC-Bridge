using CdcBridge.Application.DI;
using CdcBridge.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCdcBridge(builder.Configuration);

var host = builder.Build();

// Автоматическое применение миграций при старте
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CdcBridgeDbContext>();
    dbContext.Database.Migrate();
}


host.Run();