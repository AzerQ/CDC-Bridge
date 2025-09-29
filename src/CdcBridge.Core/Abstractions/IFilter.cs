using System;
using System.Text.Json;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для фильтра событий изменений.
/// Определяет контракт для отбора событий изменений на основе заданных условий.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Уникальное имя фильтра.
    /// Используется для идентификации в системе и ссылок из конфигурации получателей.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Проверяет, соответствует ли событие изменения условиям фильтра.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для проверки.</param>
    /// <param name="parameters">Параметры конфигурации фильтра (например, выражения JsonPath, условия и т.д.).</param>
    /// <returns><c>true</c>, если событие соответствует условиям фильтра; иначе <c>false</c>.</returns>
    public bool IsMatch(TrackedChange trackedChange, JsonElement parameters);
}
