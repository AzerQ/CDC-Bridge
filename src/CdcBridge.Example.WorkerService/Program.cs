using CdcBridge.Example.WorkerService;
using CdcBridge.Example.WorkerService.models;
using CdcBridge.Example.WorkerService.services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<DataGenerator>();

AppMode appMode = builder.Configuration.GetSection("AppMode").Get<AppMode>();

AddHostedServicesByAppMode(appMode, builder.Services);

var host = builder.Build();
host.Run();

void AddHostedServicesByAppMode(AppMode currentAppMode, IServiceCollection services)
{
    switch (currentAppMode)
    {
        case AppMode.Producer:
            builder.Services.AddHostedService<Producer>();
            break;
        
        case AppMode.Consumer:
            builder.Services.AddHostedService<Consumer>();
            break;
        
        case AppMode.ProducerAndConsumer:
            builder.Services
                .AddHostedService<Consumer>()
                .AddHostedService<Producer>();
            break;
        
        default:
            throw new ArgumentOutOfRangeException(nameof(currentAppMode), currentAppMode, null);
    }
}