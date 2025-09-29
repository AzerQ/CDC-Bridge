using System.Text.Json.Nodes;

namespace CdcBridge.Core.Models;

/// <summary>
/// Полезная нагрузка изменения: значения полей до и после операции.
/// </summary>
public class ChangeData
{
    /// <summary>
    /// Снимок записи до изменения. Может быть <c>null</c> для операций вставки.
    /// </summary>
    public JsonNode? Old { get; set; }

    /// <summary>
    /// Снимок записи после изменения. Может быть <c>null</c> для операций удаления.
    /// </summary>
    public JsonNode? New { get; set; }
}
