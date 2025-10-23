using CdcBridge.Host.Api.DTOs;

namespace CdcBridge.Host.Api.Services;

/// <summary>
/// ��������� ������� ��� ������ � ������, ����������� � SQLite.
/// </summary>
public interface ILogsService
{
 /// <summary>
    /// �������� ������ ����� � ����������� � ����������.
    /// </summary>
    /// <param name="query">��������� ������� � ��������� � ����������� ���������.</param>
    /// <returns>�������������� ��������� � ������.</returns>
    Task<PagedResultDto<LogEntryDto>> GetLogsAsync(LogQueryDto query);
}
