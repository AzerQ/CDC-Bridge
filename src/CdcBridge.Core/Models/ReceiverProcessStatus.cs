namespace CdcBridge.Core.Models;

/// <summary>
/// Статус обработки события изменения получателем.
/// </summary>
public enum ReceiverProcessStatus
{
    /// <summary>
    /// Событие успешно обработано и отправлено получателю.
    /// </summary>
    Success,
    
    /// <summary>
    /// Произошла ошибка при обработке или отправке события.
    /// Подробности ошибки содержатся в свойстве ErrorDescription.
    /// </summary>
    Failure
}