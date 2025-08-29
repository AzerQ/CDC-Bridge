using Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "yourissuer",
            ValidAudience = "youraudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here"))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.UseAuthentication();
app.UseAuthorization();

// Регистрируем CoreService
builder.Services.AddSingleton<CoreService>();

// Определяем маршруты
app.MapGet("/sources", (CoreService service) => service.GetLoadedSources()) // Предполагаем метод в CoreService
    .WithName("GetSources")
    .WithOpenApi()
    .RequireAuthorization();

app.MapPost("/sources", async (CoreService service, string pluginName) => {
    await service.LoadSourcePluginAsync(pluginName);
    return Results.Ok();
})
    .WithName("AddSource")
    .WithOpenApi()
    .RequireAuthorization();

// Эндпоинты для управления приемниками
app.MapGet("/sinks", (CoreService service) => service.GetLoadedSinks())
    .WithName("GetSinks")
    .WithOpenApi()
    .RequireAuthorization();

app.MapPost("/sinks", async (CoreService service, string pluginName) => {
    await service.LoadSinkPluginAsync(pluginName);
    return Results.Ok();
})
    .WithName("AddSink")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
