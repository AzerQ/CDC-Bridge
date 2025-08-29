# Техническое задание (ТЗ) на разработку системы **CDC Event Dispatcher**

## 1. Назначение системы
Система предназначена для:
- Подписки на изменения в таблицах БД (через CDC SQL Server).  
- Обработки и фильтрации событий.  
- Отправки этих событий во внешние системы (например: Webhook).  
- Модульной расширяемости с помощью **Prise .NET Plugin System**.  

Первоочередная задача MVP:
1. Реализация ядра (Core + API).  
2. Поддержка **CDC в MS SQL Server** (как отдельный Source Plugin).  
3. Поддержка WebHook (HTTP POST) как модуля доставки.  

---

## 2. Архитектура системы

### 2.1 Ядро (Core Service)
- Управляет загрузкой плагинов через **Prise**.  
- Содержит **Scheduler** для регулярного запуска опроса источников.  
- Выполняет:
  - Опрос источников → получение событий.  
  - Фильтрацию.  
  - Применение трансформаций (JSON).  
  - Отправку через плагины приемников.  
- Ведет системный журнал в БД.  

### 2.2 Система плагинов (Prise)
Плагины подключаются по контракту (через интерфейсы).  
Загружаются динамически из указанной папки (`/plugins`) или из внешнего NuGet.  

#### Базовые интерфейсы
```csharp
public interface ISourcePlugin
{
    string Name { get; }
    Task<IEnumerable<ChangeEvent>> GetChangesAsync(CancellationToken token);
}

public interface ISinkPlugin
{
    string Name { get; }
    Task<DeliveryResult> SendAsync(ChangeEvent change, CancellationToken token);
}

public interface IFilterPlugin
{
    bool Apply(ChangeEvent change);
}

public class ChangeEvent
{
    public string Table { get; set; }
    public ChangeType Type { get; set; } // Insert, Update, Delete
    public object OldData { get; set; }
    public object NewData { get; set; }
    public DateTimeOffset EventTime { get; set; }
}
```

#### Особенности Prise:
- **Изоляция версий** (плагины можно писать на разных версиях .NET).  
- Контракты описываются в отдельной сборке (`Plugin.Contracts`).  
- Плагины используют `[Plugin]` атрибут и загружаются через `PrisePluginLoader`.  

---

### 2.3 Source Plugin: CDC SQL Server
- Реализует `ISourcePlugin`.  
- Использует встроенный CDC SQL Server (`sys.fn_cdc_get_all_changes_<capture_instance>`) для выборки изменений.  
- Конфигурация:
  - Строка подключения (зашифрованная).  
  - Имя таблицы.  
  - Интервал опроса.  
  - Список отслеживаемых полей.  

Пример задачи:  
Каждые 15 секунд выполнять SQL-запрос к таблице CDC, формировать `ChangeEvent` объекты и передавать их в ядро.

---

### 2.4 Sink Plugin: Webhook
- Реализует `ISinkPlugin`.  
- Делает HTTP POST на заданный URL.  
- Конфигурация:  
  - URL.  
  - Заголовки запроса.  
  - Таймаут.  
  - Количество повторных попыток.  

Размер тела запроса (Payload):  
```json
{
  "operation": "Insert",
  "table": "Users",
  "oldData": { ... },
  "newData": { ... },
  "metadata": {
    "timestamp": "2024-02-20T12:00:00",
    "server": "primary-sql"
  }
}
```

Результат:  
- Успех (HTTP 200–299).  
- Ошибка с описанием + фиксируется в журнале.  

---

### 2.5 Журнал событий (DB)
Для каждого события хранится:
- Идентификатор события.  
- Исходные данные (`oldData`, `newData`).  
- Тип операции.  
- Источник (plugin).  
- Приемник (plugin).  
- Время отправки.  
- Статус (успех/ошибка).  
- Количество ретраев.  

---

### 2.6 API (админский доступ)
**.NET 8 Web API** (REST, JSON, JWT аутентификация).  

Методы API:
- `POST /sources` – добавить источник.  
- `GET /sources` – получить список источников.  
- `POST /sinks` – добавить приемник.  
- `GET /sinks` – список приемников.  
- `GET /events` – журнал событий.  
- `POST /filters` – настройка фильтрации.  
- `PUT /tasks/{id}/pause` – остановка обработки.  

---

### 2.7 Админская панель (Svelte + Typescript)
Функционал:
- Конфигурирование источников (SQL Server CDC).  
- Конфигурирование приемников (Webhook).  
- Управление фильтрами.  
- Просмотр журнала событий.  
- Проверка статуса системы.  

---

## 3. MVP Релиз (первый этап)
1. Реализация **Core Service** с интеграцией Prise.  
2. Реализация **SQL Server CDC Source Plugin**.  
3. Реализация **Webhook Sink Plugin**.  
4. Журнал событий в SQLite (как стартовая БД).  
5. API (Добавить/Удалить источник и приемник, журнал).  
6. Использование JWT для авторизации.  

---

## 4. Roadmap
- Фильтры как плагины (через Prise).  
- Поддержка PostgreSQL (через Logical Replication).  
- Поддержка Kafka/RabbitMQ Sink плагинов.  
- UI для трансформаций JSON.  
- RBAC (роли и права пользователей).  

---

✅ В результате **первая версия (MVP)**:  
- Ядро (Core) написанное на C# .NET 8.  
- Плагин на Prise для CDC в SQL Server.  
- Плагин на Prise для Webhook.  
- API на .NET 8 с JWT.  
- Простая админка (Svelte/TS).  
- Результат можно запускать как в **IIS**, так и в **Docker**.  