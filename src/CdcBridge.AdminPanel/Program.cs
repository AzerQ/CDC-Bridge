using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CdcBridge.AdminPanel;
using CdcBridge.AdminPanel.Services;
using CdcBridge.ApiClient.Extensions;
using MudBlazor.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Blazored LocalStorage for storing API key
builder.Services.AddBlazoredLocalStorage();

// Add authentication state service
builder.Services.AddScoped<AuthenticationStateService>();

// Get API base URL from configuration or use default
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";

// Add CDC Bridge API client
builder.Services.AddCdcBridgeApiClient(
    apiBaseUrl,
    async () =>
    {
        var authService = builder.Services.BuildServiceProvider().GetRequiredService<AuthenticationStateService>();
        return await authService.GetApiKeyAsync() ?? string.Empty;
    });

await builder.Build().RunAsync();
