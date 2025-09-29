using System;

namespace CdcBridge.Core.Models;

/// <summary>
/// Модель подключения к источнику данных
/// </summary>
public class Connection
{
    /// <summary>
    /// Наименование подключения
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Описание подключения
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Строка подключения
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Тип подключения (например, "SqlServer", "PostgreSql" и т.д.)
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Активно ли подключение
    /// </summary>
    public bool Active { get; set; } = true;

}
