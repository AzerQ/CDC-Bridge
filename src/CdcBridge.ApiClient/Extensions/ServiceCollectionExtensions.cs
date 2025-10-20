using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;
using System.Net.Http.Headers;

namespace CdcBridge.ApiClient.Extensions;

/// <summary>
/// Extension methods for registering CDC Bridge API client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет CDC Bridge API клиент в DI container.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="baseUrl">Base URL API (например, "https://localhost:5001").</param>
    /// <param name="getToken">Функция для получения JWT токена.</param>
    /// <returns>Service collection для цепочки вызовов.</returns>
    public static IServiceCollection AddCdcBridgeApiClient(
        this IServiceCollection services,
        string baseUrl,
        Func<Task<string>> getToken)
    {
        var refitSettings = new RefitSettings
        {
            AuthorizationHeaderValueGetter = async (request, cancellationToken) => await getToken()
        };

        // Add retry policy using Polly
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        services.AddRefitClient<Interfaces.IMetricsApi>(refitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
            .AddPolicyHandler(retryPolicy);

        return services;
    }
}
