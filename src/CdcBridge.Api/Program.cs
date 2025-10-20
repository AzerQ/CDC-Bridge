using CdcBridge.Api.Services;
using CdcBridge.Configuration;
using CdcBridge.Logging;
using CdcBridge.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add structured logging
builder.Services.AddStructuredLogging(builder.Configuration);

// Add DbContext factory for CDC Bridge database
builder.Services.AddDbContextFactory<CdcBridgeDbContext>(options =>
{
    var dbPath = builder.Configuration.GetValue<string>("Persistence:DbFilePath") ?? "data/cdc_bridge.db";
    options.UseSqlite($"Data Source={dbPath}");
});

// Add CDC Bridge configuration
var configPath = builder.Configuration.GetValue<string>("CdcBridge:ConfigurationPath") ?? "cdc-settings.yaml";

// Create YAML preprocessors
var preprocessors = new List<CdcBridge.Configuration.Preprocessing.IYamlPreprocessor>
{
    new CdcBridge.Configuration.Preprocessing.ConfigurationValuePreprocessor(builder.Configuration),
    new CdcBridge.Configuration.Preprocessing.FileContentPreprocessor()
};

// Create YAML processor for configuration macros
var yamlProcessor = new CdcBridge.Configuration.Preprocessing.YamlProcessor(preprocessors);
var configBuilder = new CdcConfigurationContextBuilder(yamlProcessor);

// Проверяем существование файла конфигурации
if (File.Exists(configPath))
{
    configBuilder.AddConfigurationFromFile(configPath);
    var cdcConfig = configBuilder.Build();
    builder.Services.AddSingleton<ICdcConfigurationContext>(cdcConfig);
}
else
{
    // Если файла нет, создаем пустую конфигурацию для работы API
    builder.Logging.AddConsole().AddDebug();
    builder.Services.AddSingleton<ICdcConfigurationContext>(sp =>
    {
        var emptyConfig = new CdcConfigurationContext(new CdcBridge.Configuration.Models.CdcSettings
        {
            Connections = Array.Empty<CdcBridge.Configuration.Models.Connection>(),
            TrackingInstances = Array.Empty<CdcBridge.Configuration.Models.TrackingInstance>(),
            Receivers = Array.Empty<CdcBridge.Configuration.Models.Receiver>()
        });
        return emptyConfig;
    });
}

// Add services
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<EventsService>();
builder.Services.AddScoped<LogsService>();

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

// Configure JWT authentication
var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key") ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer") ?? "CdcBridge.Api";
var jwtAudience = builder.Configuration.GetValue<string>("Jwt:Audience") ?? "CdcBridge.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CDC Bridge API",
        Version = "v1",
        Description = "Read-only API для мониторинга и управления CDC Bridge"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
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

// Configure the HTTP request pipeline
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

