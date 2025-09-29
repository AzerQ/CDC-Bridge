using System;
using System.Text.Json;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для получателя уведомлений о событиях изменений.
/// Определяет контракт для отправки событий изменений во внешние системы.
/// </summary>
public interface IReceiver
{
    /// <summary>
    /// Уникальное имя получателя.
    /// Используется для идентификации в системе и ссылок из конфигурации.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Асинхронно отправляет событие изменения во внешнюю систему.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для отправки.</param>
    /// <param name="parameters">Параметры конфигурации для отправки (URL, заголовки, настройки подключения и т.д.).</param>
    /// <returns>Результат обработки, включающий статус успеха/неудачи и описание ошибки при необходимости.</returns>
    Task<ReceiverProcessResult> SendAsync(TrackedChange trackedChange, JsonElement parameters);
}
