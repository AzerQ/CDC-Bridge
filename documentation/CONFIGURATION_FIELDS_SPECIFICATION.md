# Полная спецификация полей конфигурации CDC Bridge

## Обзор покрытия схемы

? **Все поля из `appsettings.json` покрыты JSON Schema**

## Полная структура конфигурации

### 1. Serilog (Логирование)

#### Serilog.MinimumLevel
**Тип:** object  
**Описание:** Minimum logging level configuration

##### Serilog.MinimumLevel.Default
- **Тип:** string
- **Допустимые значения:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`
- **По умолчанию:** `Information`
- **Описание:** Default minimum logging level for all namespaces

##### Serilog.MinimumLevel.Override
- **Тип:** object
- **Описание:** Override minimum logging levels for specific namespaces
- **Ключи:** Название namespace (например, "Microsoft", "System")
- **Значения:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`

**Пример:**
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
    }
  }
}
```

#### Serilog.Enrich
- **Тип:** array[string]
- **Допустимые значения:** 
  - `FromLogContext`
  - `WithThreadId`
  - `WithMachineName`
  - `WithEnvironmentName`
  - `WithProcessId`
  - `WithProcessName`
- **По умолчанию:** `["FromLogContext", "WithThreadId", "WithMachineName"]`
- **Описание:** List of enrichers to add contextual information to log events

**Пример:**
```json
{
  "Serilog": {
    "Enrich": ["FromLogContext", "WithThreadId", "WithMachineName"]
  }
}
```

#### Serilog.WriteTo
- **Тип:** array[object]
- **Описание:** List of sinks to write log events to

##### WriteTo[].Name
- **Тип:** string
- **Обязательное:** Да
- **Примеры:** `Console`, `SQLite`, `File`, `Seq`
- **Описание:** Name of the sink (Console, SQLite, File, Seq, etc.)

##### WriteTo[].Args (Console)
- **outputTemplate** (string): Output template for Console sink
  - **Пример:** `[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}`

##### WriteTo[].Args (SQLite)
- **sqliteDbPath** (string): Path to SQLite database file
  - **Примеры:** `data/logs.db`, `/var/logs/app.db`
- **tableName** (string): Table name in database
  - **По умолчанию:** `Logs`
- **restrictedToMinimumLevel** (string): Minimum log level for this sink
  - **Допустимые значения:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`
- **storeTimestampInUtc** (boolean): Store timestamps in UTC
  - **По умолчанию:** `true`

##### WriteTo[].Args (File)
- **path** (string): File path for logs
  - **Примеры:** `logs/log-.txt`, `/var/logs/app-.log`
- **rollingInterval** (string): Rolling interval
  - **Допустимые значения:** `Infinite`, `Year`, `Month`, `Day`, `Hour`, `Minute`
  - **По умолчанию:** `Day`
- **retainedFileCountLimit** (integer): Number of files to retain
  - **Минимум:** 1
  - **По умолчанию:** 31
- **outputTemplate** (string): Output template

##### WriteTo[].Args (Seq)
- **serverUrl** (string): Seq server URL
  - **Примеры:** `http://localhost:5341`, `http://seq:5341`
- **apiKey** (string): API key for Seq

**Пример:**
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
 "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}"
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

---

### 2. Logging (Legacy ASP.NET Core)

#### Logging.LogLevel
- **Тип:** object
- **Описание:** Log levels for different categories (legacy, prefer Serilog)
- **Ключи:** Категория логирования
- **Значения:** `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

**Пример:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
  "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```

---

### 3. Intervals

#### Intervals.ChangesDelayIntervalInSeconds
- **Тип:** integer
- **Обязательное:** Да
- **Минимум:** 1
- **Максимум:** 3600
- **По умолчанию:** 13
- **Описание:** Delay interval in seconds for processing changes. Used for throttling change detection.

**Пример:**
```json
{
  "Intervals": {
  "ChangesDelayIntervalInSeconds": 13
  }
}
```

---

### 4. ConnectionStrings

#### ConnectionStrings.default
- **Тип:** string
- **Описание:** Default SQL Server connection string for CDC tracking. Should point to the database with enabled CDC.
- **Примеры:**
  - `Data Source=localhost;Initial Catalog=mydb;Integrated Security=True`
  - `Data Source=server;Initial Catalog=db;User ID=user;Password=pass;Encrypt=True;TrustServerCertificate=True`

#### ConnectionStrings.[custom]
- **Тип:** string
- **Описание:** Additional named connection strings for multiple CDC sources

**Пример:**
```json
{
  "ConnectionStrings": {
    "default": "Data Source=server;Initial Catalog=db;User ID=sa;Password=Pass@123",
    "secondary": "Data Source=server2;Initial Catalog=db2;User ID=sa;Password=Pass@123"
  }
}
```

---

### 5. CdcBridge (Основная конфигурация)

#### CdcBridge.ConfigurationPath
- **Тип:** string
- **Обязательное:** Да
- **По умолчанию:** `cdc-settings.yaml`
- **Описание:** Path to the YAML configuration file containing CDC tracking instances, receivers, filters, and transformers. This file defines what tables to track and where to send changes.
- **Примеры:**
  - `cdc-settings.yaml`
  - `config/cdc-config.yaml`
- `/etc/cdc-bridge/config.yaml`

#### CdcBridge.WorkersConfiguration
- **Тип:** object
- **Обязательное:** Да
- **Описание:** Configuration for background worker processes that poll for changes and deliver them

##### WorkersConfiguration.ReceiverWorker
- **Тип:** object
- **Обязательное:** Да
- **Описание:** Configuration for receiver workers that deliver buffered changes to endpoints (webhooks, APIs, etc.)

###### ReceiverWorker.PollingIntervalMs
- **Тип:** integer
- **Обязательное:** Да
- **Минимум:** 100
- **Максимум:** 60000
- **По умолчанию:** 5000
- **Описание:** Polling interval in milliseconds for checking pending changes to deliver. Lower values mean faster delivery but higher CPU usage.

###### ReceiverWorker.BatchSize
- **Тип:** integer
- **Обязательное:** Да
- **Минимум:** 1
- **Максимум:** 1000
- **По умолчанию:** 200
- **Описание:** Maximum number of changes to process in a single batch per receiver. Higher values improve throughput but increase memory usage.

##### WorkersConfiguration.CleanupWorker
- **Тип:** object
- **Обязательное:** Да
- **Описание:** Configuration for cleanup worker that removes old successfully delivered events from the buffer to prevent database growth

###### CleanupWorker.CleanupIntervalHours
- **Тип:** integer
- **Обязательное:** Да
- **Минимум:** 1
- **Максимум:** 168 (1 неделя)
- **По умолчанию:** 6
- **Описание:** Interval in hours between cleanup operations. Cleanup runs periodically to remove old events.

###### CleanupWorker.BufferTimeToLiveHours
- **Тип:** integer
- **Обязательное:** Да
- **Минимум:** 1
- **Максимум:** 8760 (1 год)
- **По умолчанию:** 24
- **Описание:** Time in hours to keep successfully delivered events in the buffer before cleanup. Events older than this will be deleted if successfully delivered to all receivers.

**Пример:**
```json
{
"CdcBridge": {
    "ConfigurationPath": "cdc-settings.yaml",
    "WorkersConfiguration": {
  "ReceiverWorker": {
        "PollingIntervalMs": 5000,
        "BatchSize": 200
      },
      "CleanupWorker": {
        "CleanupIntervalHours": 6,
        "BufferTimeToLiveHours": 24
      }
    }
  }
}
```

---

### 6. Persistence

#### Persistence.DbFilePath
- **Тип:** string
- **Обязательное:** Да
- **По умолчанию:** `data/cdc_bridge.db`
- **Описание:** Path to the SQLite database file for storing buffered changes, delivery statuses, and CDC tracking state. Database is created automatically if it doesn't exist.
- **Примеры:**
  - `data/cdc_bridge.db`
  - `/var/data/cdc.db`
  - `C:\ProgramData\CdcBridge\cdc.db`

**Пример:**
```json
{
  "Persistence": {
    "DbFilePath": "data/cdc_bridge.db"
  }
}
```

---

### 7. ApiKeys

#### ApiKeys.MasterPassword
- **Тип:** string
- **Обязательное:** Да
- **Минимальная длина:** 8
- **По умолчанию:** `CHANGE_THIS_MASTER_PASSWORD_IN_PRODUCTION`
- **Описание:** Master password for initial API key creation and management. MUST be changed in production! Used to create the first admin API key via /api/Admin/initialize endpoint.
- **Примеры:**
  - `MySecurePassword123!`
  - `P@ssw0rd!2024`

**?? ВАЖНО:** Обязательно измените этот пароль в production!

**Пример:**
```json
{
  "ApiKeys": {
    "MasterPassword": "MySecurePassword123!"
  }
}
```

---

### 8. AllowedHosts

- **Тип:** string
- **По умолчанию:** `*`
- **Описание:** Semicolon-separated list of allowed host names for the application. Use '*' to allow all hosts (not recommended for production).
- **Примеры:**
  - `*`
  - `localhost;example.com`
  - `api.mycompany.com;backup.mycompany.com`

**Пример:**
```json
{
  "AllowedHosts": "localhost;api.example.com"
}
```

---

## Карта обязательных полей

### ? Обязательные на верхнем уровне
- `CdcBridge` (object)
- `Persistence` (object)
- `ApiKeys` (object)

### ? Обязательные в CdcBridge
- `CdcBridge.ConfigurationPath` (string)
- `CdcBridge.WorkersConfiguration` (object)
- `CdcBridge.WorkersConfiguration.ReceiverWorker` (object)
- `CdcBridge.WorkersConfiguration.ReceiverWorker.PollingIntervalMs` (integer)
- `CdcBridge.WorkersConfiguration.ReceiverWorker.BatchSize` (integer)
- `CdcBridge.WorkersConfiguration.CleanupWorker` (object)
- `CdcBridge.WorkersConfiguration.CleanupWorker.CleanupIntervalHours` (integer)
- `CdcBridge.WorkersConfiguration.CleanupWorker.BufferTimeToLiveHours` (integer)

### ? Обязательные в Persistence
- `Persistence.DbFilePath` (string)

### ? Обязательные в ApiKeys
- `ApiKeys.MasterPassword` (string)

### ? Обязательные в Intervals
- `Intervals.ChangesDelayIntervalInSeconds` (integer)

### ? Обязательные в Serilog.WriteTo[]
- `WriteTo[].Name` (string)

---

## Карта опциональных полей

### Опциональные на верхнем уровне
- `Serilog` (object) - но рекомендуется
- `Logging` (object) - legacy
- `Intervals` (object)
- `ConnectionStrings` (object)
- `AllowedHosts` (string)

### Опциональные в Serilog
- `Serilog.MinimumLevel` (object)
- `Serilog.Enrich` (array)
- `Serilog.WriteTo` (array)

---

## Примеры валидации

### ? Валидная минимальная конфигурация
```json
{
  "CdcBridge": {
    "ConfigurationPath": "cdc-settings.yaml",
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": 5000,
        "BatchSize": 200
      },
      "CleanupWorker": {
        "CleanupIntervalHours": 6,
        "BufferTimeToLiveHours": 24
      }
    }
  },
  "Persistence": {
    "DbFilePath": "data/cdc_bridge.db"
  },
  "ApiKeys": {
    "MasterPassword": "SecurePass123!"
  }
}
```

### ? Невалидная конфигурация - отсутствует обязательное поле
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": 5000,
        "BatchSize": 200
    }
  }
  }
  // ? Отсутствует CdcBridge.ConfigurationPath
}
```

### ? Невалидная конфигурация - неверный тип
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": "5000",  // ? Должно быть number, а не string
   "BatchSize": 200
      }
    }
  }
}
```

### ? Невалидная конфигурация - нарушение ограничений
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": 50,  // ? Минимум 100
        "BatchSize": 2000  // ? Максимум 1000
      }
    }
  }
}
```

---

## Итого

**Общее количество полей в схеме:** 30+

**Покрытие:**
- ? Serilog (6 основных свойств + детали для каждого sink)
- ? Logging (legacy)
- ? Intervals (1 поле)
- ? ConnectionStrings (dynamic)
- ? CdcBridge (8 полей)
- ? Persistence (1 поле)
- ? ApiKeys (1 поле)
- ? AllowedHosts (1 поле)

**Статус:** ? **Все поля из `appsettings.json` покрыты JSON Schema**
