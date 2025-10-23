using CdcBridge.Host.Api.DTOs;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// Интерфейс сервиса для получения метрик системы CDC Bridge.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Получает общие метрики системы.
    /// </summary>
    /// <returns>Объект с метриками системы.</returns>
    Task<MetricsDto> GetMetricsAsync();
}
