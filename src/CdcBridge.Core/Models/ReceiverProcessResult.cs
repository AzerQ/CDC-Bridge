using System;

namespace CdcBridge.Core.Models;

/// <summary>
/// Результат обработки события изменения получателем.
/// Содержит информацию о статусе выполнения операции отправки и детали ошибки при необходимости.
/// </summary>
public class ReceiverProcessResult
{
    /// <summary>
    /// Статус обработки события получателем.
    /// </summary>
    public ReceiverProcessStatus Status { get; set; }

    /// <summary>
    /// Описание ошибки, если обработка завершилась неудачей.
    /// Содержит подробную информацию о причине сбоя для последующей диагностики.
    /// </summary>
    public string? ErrorDescription { get; set; }
}
