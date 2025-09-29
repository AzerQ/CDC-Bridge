using System;
using System.Text.Json.Nodes;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для трансформера событий изменений.
/// Определяет контракт для преобразования полезной нагрузки события перед отправкой получателям.
/// </summary>
/// <typeparam name="TParameters">Тип параметров конфигурации трансформера.</typeparam>
public interface ITransformer<TParameters>
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
    public JsonNode Transform(TrackedChange trackedChange, TParameters parameters);
}
