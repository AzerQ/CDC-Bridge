using System;

namespace CdcBridge.Core.Models;

/// <summary>
/// Модель отслеживаемого объекта
/// </summary>
public class TrackingInstance
{
    /// <summary>
    /// Название исходной таблицы
    /// </summary>
    public required string SourceTable { get; set; }

    /// <summary>
    /// Cписок колонок, которые необходимо захватывать
    /// </summary>
    public required List<string> CapturedColumns { get; set; }

    /// <summary>
    /// Описание отслеживаемого объекта
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Имя подключения к источнику данных
    /// </summary>
    public required string Connection { get; set; }

    /// <summary>
    /// Активно ли отслеживание для этого объекта
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Интервал проверки изменений в секундах
    /// </summary>
    public int CheckIntervalInSeconds { get; set; }

}
