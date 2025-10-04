namespace CdcBridge.Core.Models;

/// <summary>
/// Запрос на получение порции новых данных по изменениям в таблице
/// </summary>
public record CdcRequest
{
    /// <summary>
    /// Флаг последней прочитанной строки
    /// </summary>
    public string? LastRowFlag { get; set; }

    /// <summary>
    /// Дата последней прочитанной строки
    /// </summary>
    public DateTime? LastReadRowDate { get; set; }
}