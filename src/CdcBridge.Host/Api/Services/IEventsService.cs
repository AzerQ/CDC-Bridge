using CdcBridge.Host.Api.DTOs;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// ��������� ������� ��� ������ � ��������� ��������� ������.
/// </summary>
public interface IEventsService
{
    /// <summary>
    /// �������� ������ ������� � ����������� � ����������.
    /// </summary>
    /// <param name="query">��������� ������� � ��������� � ����������� ���������.</param>
    /// <returns>�������������� ��������� � ���������.</returns>
    Task<PagedResultDto<EventDto>> GetEventsAsync(EventQueryDto query);

    /// <summary>
    /// �������� ��������� ���������� � ���������� �������.
    /// </summary>
    /// <param name="id">������������� �������.</param>
    /// <returns>��������� ���������� � ������� ��� null, ���� ������� �� �������.</returns>
    Task<EventDto?> GetEventByIdAsync(Guid id);
}
