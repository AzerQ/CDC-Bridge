using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CdcBridge.Core.Models.Configuration;

/// <summary>
/// Модель фильтра
/// </summary>
public class Filter
{
    /// <summary>
    /// Наименование фильтра
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Описание фильтра
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Наименование отслеживаемого объекта, к которому применяется фильтр
    /// </summary>
    public required string TrackingInstance { get; set; }

    /// <summary>
    /// Тип фильтра
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Параметры фильтра
    /// </summary>
    public JsonElement? Parameters { get; set; }

}
