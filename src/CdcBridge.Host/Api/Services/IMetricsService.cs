using CdcBridge.Host.Api.DTOs;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// ��������� ������� ��� ��������� ������ ������� CDC Bridge.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// �������� ����� ������� �������.
    /// </summary>
    /// <returns>������ � ��������� �������.</returns>
    Task<MetricsDto> GetMetricsAsync();
}
