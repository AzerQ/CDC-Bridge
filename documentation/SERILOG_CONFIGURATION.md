# Конфигурация структурированного логирования Serilog

## Обзор

Система логирования в CDC Bridge использует Serilog с конфигурацией через `appsettings.json`. Все настройки логирования читаются из конфигурации, что позволяет гибко управлять уровнями логирования без перекомпиляции кода.

## Миграция от хардкода к конфигурации

### Было (хардкод):
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
.MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .WriteTo.Console(...)
    .WriteTo.SQLite(...)
    .CreateLogger();
```

### Стало (конфигурация):
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

## Структура конфигурации

### appsettings.json (Production)
```json
{
  "Serilog": {
    "MinimumLevel": {
   "Default": "Information",
      "Override": {
   "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithThreadId", "WithMachineName" ],
    "WriteTo": [
{
        "Name": "Console",
"Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "SQLite",
        "Args": {
          "sqliteDbPath": "data/logs.db",
    "tableName": "Logs",
      "restrictedToMinimumLevel": "Information",
        "storeTimestampInUtc": true
   }
      }
    ]
  }
}
```

### appsettings.Development.json (Development)
```json
{
  "Serilog": {
  "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
 "Microsoft.EntityFrameworkCore": "Information",
        "System": "Information"
      }
    },
    "WriteTo": [
 {
        "Name": "Console",
        "Args": {
      "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "SQLite",
        "Args": {
    "sqliteDbPath": "data/logs.db",
    "tableName": "Logs",
          "restrictedToMinimumLevel": "Debug",
        "storeTimestampInUtc": true
        }
      }
    ]
  }
}
```

## Параметры конфигурации

### MinimumLevel

#### Default
Минимальный уровень логирования по умолчанию для всех источников.

**Возможные значения:**
- `Verbose` - максимальная детализация
- `Debug` - отладочная информация
- `Information` - информационные сообщения
- `Warning` - предупреждения
- `Error` - ошибки
- `Fatal` - критические ошибки

**Рекомендации:**
- **Production**: `Information`
- **Development**: `Debug`

#### Override
Переопределение уровней для конкретных пространств имен.

```json
"Override": {
  "Microsoft": "Warning",
  "Microsoft.EntityFrameworkCore": "Warning",
  "System": "Warning"
}
```

### Enrich
Обогащение логов дополнительной информацией.

**Доступные enrichers:**
- `FromLogContext` - контекст выполнения
- `WithThreadId` - ID потока
- `WithMachineName` - имя машины
- `WithEnvironmentName` - окружение (Development, Production)
- `WithProcessId` - ID процесса
- `WithProcessName` - имя процесса

### WriteTo
Назначения для записи логов (sinks).

#### Console Sink
Вывод логов в консоль.

```json
{
  "Name": "Console",
  "Args": {
    "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
  }
}
```

**Параметры шаблона:**
- `{Timestamp:yyyy-MM-dd HH:mm:ss}` - временная метка
- `{Level:u3}` - уровень логирования (заглавными, 3 символа)
- `{Message:lj}` - сообщение (literal JSON)
- `{Properties:j}` - дополнительные свойства (JSON)
- `{NewLine}` - перенос строки
- `{Exception}` - информация об исключении

#### SQLite Sink
Сохранение логов в базу данных SQLite.

```json
{
  "Name": "SQLite",
  "Args": {
    "sqliteDbPath": "data/logs.db",
    "tableName": "Logs",
    "restrictedToMinimumLevel": "Information",
    "storeTimestampInUtc": true
  }
}
```

**Параметры:**
- `sqliteDbPath` - путь к файлу базы данных
- `tableName` - имя таблицы (обычно "Logs")
- `restrictedToMinimumLevel` - минимальный уровень для этого sink
- `storeTimestampInUtc` - хранить время в UTC

## Примеры настройки

### Включение детального логирования EF Core
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Microsoft.EntityFrameworkCore": "Debug",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

### Логирование только ошибок в Production
```json
{
  "Serilog": {
"MinimumLevel": {
  "Default": "Error",
      "Override": {
        "CdcBridge": "Information"
      }
    }
  }
}
```

### Добавление File Sink
```json
{
  "Serilog": {
    "WriteTo": [
      {
   "Name": "Console"
      },
      {
        "Name": "File",
 "Args": {
          "path": "logs/log-.txt",
    "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    }
      }
  ]
  }
}
```

Для использования File sink нужно установить пакет:
```bash
dotnet add package Serilog.Sinks.File
```

## Переменные окружения

Можно переопределять настройки через переменные окружения:

```bash
# Изменить уровень логирования
export Serilog__MinimumLevel__Default=Debug

# Изменить путь к базе данных
export Serilog__WriteTo__1__Args__sqliteDbPath=/var/logs/cdc-bridge.db
```

В Docker Compose:
```yaml
environment:
  - Serilog__MinimumLevel__Default=Information
  - Serilog__WriteTo__1__Args__sqliteDbPath=/app/data/logs.db
```

## Преимущества конфигурационного подхода

### 1. **Гибкость**
- ? Изменение уровней без перекомпиляции
- ? Разные настройки для разных окружений
- ? Легкое добавление новых sinks

### 2. **Управление**
- ? Централизованная конфигурация
- ? Переопределение через переменные окружения
- ? Поддержка конфигурационных провайдеров ASP.NET Core

### 3. **Поддержка**
- ? Не нужно изменять код для настройки логирования
- ? Упрощенное тестирование с разными уровнями
- ? Быстрое включение детального логирования для отладки

## Структура таблицы Logs в SQLite

Serilog.Sinks.SQLite автоматически создает таблицу со следующей структурой:

| Поле | Тип | Описание |
|------|-----|----------|
| Id | INTEGER PRIMARY KEY | Автоинкрементный идентификатор |
| Timestamp | TEXT | Временная метка лога (UTC) |
| Level | TEXT | Уровень логирования |
| Exception | TEXT | Информация об исключении |
| RenderedMessage | TEXT | Отформатированное сообщение |
| Properties | TEXT | Дополнительные свойства (JSON) |

## Мониторинг логов

Логи доступны через API:

```http
GET /api/Logs?page=1&pageSize=50&level=Error
Authorization: X-API-Key: your-api-key
```

Параметры:
- `page` - номер страницы
- `pageSize` - размер страницы
- `level` - фильтр по уровню (Debug, Information, Warning, Error, Fatal)
- `messageSearch` - поиск по тексту сообщения
- `fromDate` - начальная дата
- `toDate` - конечная дата

## Устранение неполадок

### Логи не записываются в SQLite
1. Проверьте права доступа к директории `data/`
2. Убедитесь, что `restrictedToMinimumLevel` не выше текущего уровня
3. Проверьте наличие пакета `Serilog.Sinks.SQLite`

### Слишком много логов
1. Повысьте `MinimumLevel.Default` до `Warning` или `Error`
2. Добавьте переопределения для шумных пространств имен
3. Настройте `restrictedToMinimumLevel` для SQLite sink

### Логи не появляются в консоли
1. Проверьте наличие Console sink в `WriteTo`
2. Убедитесь, что уровень логирования достаточно низкий

## Требуемые пакеты

```xml
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.SQLite" Version="6.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="9.0.0" />
```

## Заключение

Миграция на конфигурационный подход для Serilog обеспечивает:
- ? Гибкое управление уровнями логирования
- ? Легкую настройку для разных окружений
- ? Отсутствие необходимости в перекомпиляции
- ? Поддержку переменных окружения
- ? Соответствие лучшим практикам .NET
