# Исправление ошибки "no such table: Logs"

## Проблема

При запросе логов через API возникала ошибка:
```
SQLite Error 1: 'no such table: Logs'
```

Файл `data/logs.db` был пустым (0 байт), хотя логирование было настроено через Serilog.

## Причина

Serilog с SQLite создает таблицу `Logs` только при первой записи лога. Если логов еще не было записано или база данных была удалена/пересоздана, таблица отсутствовала, что приводило к ошибке при попытке чтения логов.

## Решение

### 1. Автоматическое создание таблицы в LogsService

Добавлен метод `EnsureLogTableExistsAsync()`, который создает таблицу `Logs` с индексами, если она не существует. Структура таблицы соответствует `Serilog.Sinks.SQLite` версии 6.0.0:

```csharp
private async Task EnsureLogTableExistsAsync(SqliteConnection connection)
{
    // Структура таблицы соответствует Serilog.Sinks.SQLite v6.0.0
    var createTableSql = @"
        CREATE TABLE IF NOT EXISTS Logs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
     Timestamp TEXT NOT NULL,
   Level TEXT NOT NULL,
            Exception TEXT,
        Message TEXT,
 Properties TEXT
        );
     
      CREATE INDEX IF NOT EXISTS IX_Logs_Timestamp ON Logs(Timestamp);
   CREATE INDEX IF NOT EXISTS IX_Logs_Level ON Logs(Level);";

    using var command = new SqliteCommand(createTableSql, connection);
    await command.ExecuteNonQueryAsync();
}
```

**Важно:** Serilog использует поле `Message` для хранения текста лога, а не `Message`.

Этот метод вызывается при каждом запросе к API логов, гарантируя наличие таблицы.

### 2. Инициализация логирования при старте

В `Program.cs` добавлен тестовый лог при запуске приложения:

```csharp
// Записываем тестовый лог для инициализации Serilog SQLite таблицы
app.Logger.LogInformation("CDC Bridge Host is starting...");
```

Это гарантирует, что Serilog создаст таблицу при первом запуске приложения.

### 3. Создание директории для базы данных

В `LogsService` добавлена проверка и создание директории для файла базы данных:

```csharp
// Убедимся, что директория существует
var directory = Path.GetDirectoryName(_logDbPath);
if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
    _logger.LogInformation("Created log database directory: {Directory}", directory);
}
```

### 4. Корректные SQL-запросы

SQL-запросы обновлены для использования `Message` вместо `Message`:

```csharp
// Поиск по сообщению
if (!string.IsNullOrEmpty(query.MessageSearch))
{
    whereConditions.Add("Message LIKE @MessageSearch");
    parameters.Add(new SqliteParameter("@MessageSearch", $"%{query.MessageSearch}%"));
}

// Выборка данных
var dataQuery = $@"
    SELECT Id, Timestamp, Level, Message, Exception, Properties 
    FROM Logs 
    {whereClause}
    ORDER BY Timestamp DESC 
    LIMIT @PageSize OFFSET @Offset";
```

## Результат

Теперь API логов:
- ✅ Автоматически создает таблицу `Logs`, если её нет
- ✅ Использует правильные имена полей (Message вместо Message)
- ✅ Корректно обрабатывает случай пустой базы данных
- ✅ Создает необходимые директории
- ✅ Возвращает пустой список, если логов еще нет, вместо ошибки

## Структура таблицы Logs

Таблица `Logs`, создаваемая Serilog.Sinks.SQLite v6.0.0:

| Поле | Тип | Описание |
|------|-----|----------|
| Id | INTEGER PRIMARY KEY | Автоинкрементный идентификатор |
| Timestamp | TEXT | Временная метка лога |
| Level | TEXT | Уровень логирования (Information, Warning, Error и т.д.) |
| Exception | TEXT | Информация об исключении (если есть) |
| Message | TEXT | Отформатированное сообщение лога |
| Properties | TEXT | Дополнительные свойства в формате JSON |

Индексы:
- `IX_Logs_Timestamp` - для быстрой сортировки по времени
- `IX_Logs_Level` - для фильтрации по уровню логирования

## Конфигурация

Путь к базе данных логов настраивается в `appsettings.json`:

```json
{
  "Logging": {
    "SqliteDbPath": "data/logs.db"
  }
}
```

По умолчанию используется `data/logs.db`.
