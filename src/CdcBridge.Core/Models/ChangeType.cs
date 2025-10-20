namespace CdcBridge.Core.Models;

/// <summary>
/// Перечень типов изменений, поддерживаемых системой CDC.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// Вставка новой записи (предыдущее состояние отсутствует).
    /// </summary>
    Insert,
    /// <summary>
    /// Обновление существующей записи (есть состояние «до» и «после»).
    /// </summary>
    Update,
    /// <summary>
    /// Удаление записи (состояние «после» отсутствует).
    /// </summary>
    Delete
}
