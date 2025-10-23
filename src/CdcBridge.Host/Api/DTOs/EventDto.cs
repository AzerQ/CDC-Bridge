namespace CdcBridge.Host.Api.DTOs;

/// <summary>
/// DTO для отображения события изменения данных.
/// </summary>
public class EventDto
{
    /// <summary>
    /// Уникальный идентификатор события.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя экземпляра отслеживания.
    /// </summary>
    public required string TrackingInstanceName { get; set; }

    /// <summary>
    /// Метка строки (LSN или аналог).
    /// </summary>
    public required string RowLabel { get; set; }

    /// <summary>
    /// Время помещения в буфер.
    /// </summary>
    public DateTime BufferedAt { get; set; }

    /// <summary>
    /// Тип операции (Insert, Update, Delete).
    /// </summary>
    public string? OperationType { get; set; }

    /// <summary>
    /// Статусы доставки для каждого получателя.
    /// </summary>
    public List<EventDeliveryStatusDto> DeliveryStatuses { get; set; } = new();
}

/// <summary>
/// DTO для статуса доставки события конкретному получателю.
/// </summary>
public class EventDeliveryStatusDto
{
    /// <summary>
    /// Имя получателя.
    /// </summary>
    public required string ReceiverName { get; set; }

    /// <summary>
    /// Статус доставки (Pending, Success, Failed).
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Количество попыток отправки.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Время последней попытки.
    /// </summary>
    public DateTime? LastAttemptAt { get; set; }

    /// <summary>
    /// Описание ошибки, если доставка не удалась.
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Время последней доставки в миллисекундах.
    /// </summary>
    public long? LastDeliveryTimeMs { get; set; }

    /// <summary>
    /// Среднее время доставки в миллисекундах.
    /// </summary>
    public double? AverageDeliveryTimeMs { get; set; }
}

/// <summary>
/// Параметры запроса для получения списка событий с фильтрацией и пагинацией.
/// </summary>
public class EventQueryDto
{
    /// <summary>
    /// Имя экземпляра отслеживания (фильтр).
    /// </summary>
    public string? TrackingInstanceName { get; set; }

    /// <summary>
    /// Имя получателя (фильтр).
    /// </summary>
    public string? ReceiverName { get; set; }

    /// <summary>
    /// Статус доставки (фильтр).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Начало диапазона времени (фильтр).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Конец диапазона времени (фильтр).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Номер страницы (начиная с 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Размер страницы.
    /// </summary>
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// DTO для пагинированного результата.
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>
    /// Список элементов на текущей странице.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Текущая страница.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
