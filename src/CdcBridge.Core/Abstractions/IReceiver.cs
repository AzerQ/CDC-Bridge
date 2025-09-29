using System;
using CdcBridge.Core.Models;

namespace CdcBridge.Core.Abstractions;

/// <summary>
/// Интерфейс для получателя уведомлений о событиях изменений.
/// Определяет контракт для отправки событий изменений во внешние системы.
/// </summary>
/// <typeparam name="TParameters">Тип параметров конфигурации получателя.</typeparam>
public interface IReceiver<TParameters>
{
    /// <summary>
    /// Асинхронно отправляет событие изменения во внешнюю систему.
    /// </summary>
    /// <param name="trackedChange">Событие изменения для отправки.</param>
    /// <param name="parameters">Параметры конфигурации для отправки (URL, заголовки, настройки подключения и т.д.).</param>
    /// <returns>Результат обработки, включающий статус успеха/неудачи и описание ошибки при необходимости.</returns>
    Task<ReceiverProcessResult> SendAsync(TrackedChange trackedChange, TParameters parameters);
}
