using System.Text.Json;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для трансформера событий изменений.
/// Определяет контракт для преобразования полезной нагрузки события перед отправкой получателям.
/// </summary>

public interface ITransformer
{
    /// <summary>
    /// Уникальное имя трансформера.
    /// Используется для идентификации в системе и ссылок из конфигурации.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Преобразует событие изменения согласно настроенным параметрам.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для преобразования.</param>
    /// <param name="parameters">Параметры конфигурации трансформера (например, выражения JSONata, шаблоны и т.д.).</param>
    /// <returns>Преобразованные данные в формате JSON.</returns>
    public JsonElement Transform(TrackedChange trackedChange, JsonElement parameters);
}
