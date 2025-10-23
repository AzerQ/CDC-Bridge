# ������ ������������ ����� ������������ CDC Bridge

## ����� �������� �����

? **��� ���� �� `appsettings.json` ������� JSON Schema**

## ������ ��������� ������������

### 1. Serilog (�����������)

#### Serilog.MinimumLevel
**���:** object  
**��������:** Minimum logging level configuration

##### Serilog.MinimumLevel.Default
- **���:** string
- **���������� ��������:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`
- **�� ���������:** `Information`
- **��������:** Default minimum logging level for all namespaces

##### Serilog.MinimumLevel.Override
- **���:** object
- **��������:** Override minimum logging levels for specific namespaces
- **�����:** �������� namespace (��������, "Microsoft", "System")
- **��������:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`

**������:**
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
- **���:** array[string]
- **���������� ��������:** 
  - `FromLogContext`
  - `WithThreadId`
  - `WithMachineName`
  - `WithEnvironmentName`
  - `WithProcessId`
  - `WithProcessName`
- **�� ���������:** `["FromLogContext", "WithThreadId", "WithMachineName"]`
- **��������:** List of enrichers to add contextual information to log events

**������:**
```json
{
  "Serilog": {
    "Enrich": ["FromLogContext", "WithThreadId", "WithMachineName"]
  }
}
```

#### Serilog.WriteTo
- **���:** array[object]
- **��������:** List of sinks to write log events to

##### WriteTo[].Name
- **���:** string
- **������������:** ��
- **�������:** `Console`, `SQLite`, `File`, `Seq`
- **��������:** Name of the sink (Console, SQLite, File, Seq, etc.)

##### WriteTo[].Args (Console)
- **outputTemplate** (string): Output template for Console sink
  - **������:** `[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}`

##### WriteTo[].Args (SQLite)
- **sqliteDbPath** (string): Path to SQLite database file
  - **�������:** `data/logs.db`, `/var/logs/app.db`
- **tableName** (string): Table name in database
  - **�� ���������:** `Logs`
- **restrictedToMinimumLevel** (string): Minimum log level for this sink
  - **���������� ��������:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`
- **storeTimestampInUtc** (boolean): Store timestamps in UTC
  - **�� ���������:** `true`

##### WriteTo[].Args (File)
- **path** (string): File path for logs
  - **�������:** `logs/log-.txt`, `/var/logs/app-.log`
- **rollingInterval** (string): Rolling interval
  - **���������� ��������:** `Infinite`, `Year`, `Month`, `Day`, `Hour`, `Minute`
  - **�� ���������:** `Day`
- **retainedFileCountLimit** (integer): Number of files to retain
  - **�������:** 1
  - **�� ���������:** 31
- **outputTemplate** (string): Output template

##### WriteTo[].Args (Seq)
- **serverUrl** (string): Seq server URL
  - **�������:** `http://localhost:5341`, `http://seq:5341`
- **apiKey** (string): API key for Seq

**������:**
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
- **���:** object
- **��������:** Log levels for different categories (legacy, prefer Serilog)
- **�����:** ��������� �����������
- **��������:** `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`

**������:**
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
- **���:** integer
- **������������:** ��
- **�������:** 1
- **��������:** 3600
- **�� ���������:** 13
- **��������:** Delay interval in seconds for processing changes. Used for throttling change detection.

**������:**
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
- **���:** string
- **��������:** Default SQL Server connection string for CDC tracking. Should point to the database with enabled CDC.
- **�������:**
  - `Data Source=localhost;Initial Catalog=mydb;Integrated Security=True`
  - `Data Source=server;Initial Catalog=db;User ID=user;Password=pass;Encrypt=True;TrustServerCertificate=True`

#### ConnectionStrings.[custom]
- **���:** string
- **��������:** Additional named connection strings for multiple CDC sources

**������:**
```json
{
  "ConnectionStrings": {
    "default": "Data Source=server;Initial Catalog=db;User ID=sa;Password=Pass@123",
    "secondary": "Data Source=server2;Initial Catalog=db2;User ID=sa;Password=Pass@123"
  }
}
```

---

### 5. CdcBridge (�������� ������������)

#### CdcBridge.ConfigurationPath
- **���:** string
- **������������:** ��
- **�� ���������:** `cdc-settings.yaml`
- **��������:** Path to the YAML configuration file containing CDC tracking instances, receivers, filters, and transformers. This file defines what tables to track and where to send changes.
- **�������:**
  - `cdc-settings.yaml`
  - `config/cdc-config.yaml`
- `/etc/cdc-bridge/config.yaml`

#### CdcBridge.WorkersConfiguration
- **���:** object
- **������������:** ��
- **��������:** Configuration for background worker processes that poll for changes and deliver them

##### WorkersConfiguration.ReceiverWorker
- **���:** object
- **������������:** ��
- **��������:** Configuration for receiver workers that deliver buffered changes to endpoints (webhooks, APIs, etc.)

###### ReceiverWorker.PollingIntervalMs
- **���:** integer
- **������������:** ��
- **�������:** 100
- **��������:** 60000
- **�� ���������:** 5000
- **��������:** Polling interval in milliseconds for checking pending changes to deliver. Lower values mean faster delivery but higher CPU usage.

###### ReceiverWorker.BatchSize
- **���:** integer
- **������������:** ��
- **�������:** 1
- **��������:** 1000
- **�� ���������:** 200
- **��������:** Maximum number of changes to process in a single batch per receiver. Higher values improve throughput but increase memory usage.

##### WorkersConfiguration.CleanupWorker
- **���:** object
- **������������:** ��
- **��������:** Configuration for cleanup worker that removes old successfully delivered events from the buffer to prevent database growth

###### CleanupWorker.CleanupIntervalHours
- **���:** integer
- **������������:** ��
- **�������:** 1
- **��������:** 168 (1 ������)
- **�� ���������:** 6
- **��������:** Interval in hours between cleanup operations. Cleanup runs periodically to remove old events.

###### CleanupWorker.BufferTimeToLiveHours
- **���:** integer
- **������������:** ��
- **�������:** 1
- **��������:** 8760 (1 ���)
- **�� ���������:** 24
- **��������:** Time in hours to keep successfully delivered events in the buffer before cleanup. Events older than this will be deleted if successfully delivered to all receivers.

**������:**
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
- **���:** string
- **������������:** ��
- **�� ���������:** `data/cdc_bridge.db`
- **��������:** Path to the SQLite database file for storing buffered changes, delivery statuses, and CDC tracking state. Database is created automatically if it doesn't exist.
- **�������:**
  - `data/cdc_bridge.db`
  - `/var/data/cdc.db`
  - `C:\ProgramData\CdcBridge\cdc.db`

**������:**
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
- **���:** string
- **������������:** ��
- **����������� �����:** 8
- **�� ���������:** `CHANGE_THIS_MASTER_PASSWORD_IN_PRODUCTION`
- **��������:** Master password for initial API key creation and management. MUST be changed in production! Used to create the first admin API key via /api/Admin/initialize endpoint.
- **�������:**
  - `MySecurePassword123!`
  - `P@ssw0rd!2024`

**?? �����:** ����������� �������� ���� ������ � production!

**������:**
```json
{
  "ApiKeys": {
    "MasterPassword": "MySecurePassword123!"
  }
}
```

---

### 8. AllowedHosts

- **���:** string
- **�� ���������:** `*`
- **��������:** Semicolon-separated list of allowed host names for the application. Use '*' to allow all hosts (not recommended for production).
- **�������:**
  - `*`
  - `localhost;example.com`
  - `api.mycompany.com;backup.mycompany.com`

**������:**
```json
{
  "AllowedHosts": "localhost;api.example.com"
}
```

---

## ����� ������������ �����

### ? ������������ �� ������� ������
- `CdcBridge` (object)
- `Persistence` (object)
- `ApiKeys` (object)

### ? ������������ � CdcBridge
- `CdcBridge.ConfigurationPath` (string)
- `CdcBridge.WorkersConfiguration` (object)
- `CdcBridge.WorkersConfiguration.ReceiverWorker` (object)
- `CdcBridge.WorkersConfiguration.ReceiverWorker.PollingIntervalMs` (integer)
- `CdcBridge.WorkersConfiguration.ReceiverWorker.BatchSize` (integer)
- `CdcBridge.WorkersConfiguration.CleanupWorker` (object)
- `CdcBridge.WorkersConfiguration.CleanupWorker.CleanupIntervalHours` (integer)
- `CdcBridge.WorkersConfiguration.CleanupWorker.BufferTimeToLiveHours` (integer)

### ? ������������ � Persistence
- `Persistence.DbFilePath` (string)

### ? ������������ � ApiKeys
- `ApiKeys.MasterPassword` (string)

### ? ������������ � Intervals
- `Intervals.ChangesDelayIntervalInSeconds` (integer)

### ? ������������ � Serilog.WriteTo[]
- `WriteTo[].Name` (string)

---

## ����� ������������ �����

### ������������ �� ������� ������
- `Serilog` (object) - �� �������������
- `Logging` (object) - legacy
- `Intervals` (object)
- `ConnectionStrings` (object)
- `AllowedHosts` (string)

### ������������ � Serilog
- `Serilog.MinimumLevel` (object)
- `Serilog.Enrich` (array)
- `Serilog.WriteTo` (array)

---

## ������� ���������

### ? �������� ����������� ������������
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

### ? ���������� ������������ - ����������� ������������ ����
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
  // ? ����������� CdcBridge.ConfigurationPath
}
```

### ? ���������� ������������ - �������� ���
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": "5000",  // ? ������ ���� number, � �� string
   "BatchSize": 200
      }
    }
  }
}
```

### ? ���������� ������������ - ��������� �����������
```json
{
  "CdcBridge": {
    "WorkersConfiguration": {
      "ReceiverWorker": {
        "PollingIntervalMs": 50,  // ? ������� 100
        "BatchSize": 2000  // ? �������� 1000
      }
    }
  }
}
```

---

## �����

**����� ���������� ����� � �����:** 30+

**��������:**
- ? Serilog (6 �������� ������� + ������ ��� ������� sink)
- ? Logging (legacy)
- ? Intervals (1 ����)
- ? ConnectionStrings (dynamic)
- ? CdcBridge (8 �����)
- ? Persistence (1 ����)
- ? ApiKeys (1 ����)
- ? AllowedHosts (1 ����)

**������:** ? **��� ���� �� `appsettings.json` ������� JSON Schema**
