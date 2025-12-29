using CdcBridge.Application.DI;
using CdcBridge.Host.Api.Services;
using CdcBridge.Host.Middleware;
using CdcBridge.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using CdcBridge.Application.DI.Logging;

void AddAllAppConfigurationFiles(ConfigurationManager config)
{
    var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    if (currentPath == null)
        return;

    var allConfigurationFiles = Directory.GetFiles(currentPath, "*.customsettings.json", SearchOption.AllDirectories);

    if (allConfigurationFiles.Length == 0)
        Console.WriteLine($"No custom configuration files were found in the applcation directory ({currentPath}).");
    
    foreach (var configurationFile in allConfigurationFiles)
    {
        config.AddJsonFile(configurationFile, optional: false, reloadOnChange: true);
        Console.WriteLine($"Added custom configuration file: {configurationFile}");
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
AddAllAppConfigurationFiles(builder.Configuration);

builder.Services.AddWindowsService();

// Add structured logging
builder.Services.AddStructuredLogging(builder.Configuration);

// Add CDC Bridge infrastructure (includes DbContext, Configuration, Background Services, etc.)
builder.Services.AddCdcBridge(builder.Configuration);

// Add API services
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<ILogsService, LogsService>();

// Add controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
      .AllowAnyMethod()
   .AllowAnyHeader();
    });
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
    Title = "CDC Bridge API",
        Version = "v1",
        Description = "API для мониторинга и управления CDC Bridge. Требуется API ключ в заголовке X-API-Key."
    });

    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication. Add your API key in the X-API-Key header. Example: \"your-api-key-here\"",
        Name = "X-API-Key",
    In = ParameterLocation.Header,
   Type = SecuritySchemeType.ApiKey,
    Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
 {
         new OpenApiSecurityScheme
   {
    Reference = new OpenApiReference
        {
      Type = ReferenceType.SecurityScheme,
         Id = "ApiKey"
      }
        },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Автоматическое применение миграций при старте
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CdcBridgeDbContext>();
    dbContext.Database.Migrate();
}

// Записываем тестовый лог для инициализации Serilog SQLite таблицы
app.Logger.LogInformation("CDC Bridge Host is starting...");

// Configure the HTTP request pipeline

// Global exception handler должен быть первым
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDC Bridge API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Используем API Key аутентификацию вместо JWT
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.MapControllers();

app.Run();
