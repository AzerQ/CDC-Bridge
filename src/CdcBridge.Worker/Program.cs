using CdcBridge.Application.DI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCdcBridge(builder.Configuration);

var host = builder.Build();
host.Run();