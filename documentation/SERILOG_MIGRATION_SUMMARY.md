# ������: �������� �������� ����������� Serilog � ������������

## ����������� ���������

### 1. �������� `StructuredLoggingExtensions.cs`
**����:**
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

**�����:**
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
  .CreateLogger();
```

### 2. �������� ����� `Serilog.Settings.Configuration`
```bash
dotnet add package Serilog.Settings.Configuration --version 9.0.0
```

### 3. �������� `Microsoft.Extensions.Configuration.Abstractions`
```bash
dotnet add package Microsoft.Extensions.Configuration.Abstractions --version 9.0.0
```

### 4. �������� `appsettings.json`
��������� ������ ������������ Serilog:
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
     "Args": { "outputTemplate": "..." }
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

### 5. �������� `appsettings.Development.json`
��������� ������������ � ������� `Debug` ��� ����������:
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
    }
  }
}
```

### 6. ������� ������������ `SERILOG_CONFIGURATION.md`
������ ����������� �� ������������ ����������� � ���������.

## ������������

### ? ��������
- ��������� ������� ����������� ��� ��������������
- ������ ��������� ��� Development � Production
- ������ ���������� ����� sinks

### ? ����������
- ���������������� ������������ � JSON
- ��������������� ����� ���������� ���������
- ��������� ���� ���������������� ����������� ASP.NET Core

### ? DevOps-friendly
```bash
# �������� ������� ����� ���������� ���������
export Serilog__MinimumLevel__Default=Debug

# � Docker Compose
environment:
  - Serilog__MinimumLevel__Default=Information
```

### ? ������������ ������ ���������
- ���������� �������� 12-factor app
- ���������� ������������ � ����
- ��������� CI/CD ���������

## ������� �������������

### �������� ��������� ����������� ��� �������
�������� � `appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Verbose",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

### ���������� ������ ������ � Production
� `appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Error",
      "Override": {
        "CdcBridge": "Warning"
      }
    }
  }
}
```

### �������������� ����� ���������� ���������
```bash
# Docker Compose
services:
  cdc-bridge:
    environment:
      - Serilog__MinimumLevel__Default=Debug
      - Serilog__WriteTo__1__Args__sqliteDbPath=/var/logs/cdc.db
```

## �������� �������������

? ��� ������������ ������� ���������
? API endpoints ��� ���������
? ��������� ������� Logs �� ����������
? ������ ����� ������� �������

## ��������� ������

��� ����������� ������ ��������� � `CdcBridge.Application.csproj`:
- ? Serilog 4.3.0
- ? Serilog.Settings.Configuration 9.0.0
- ? Serilog.Sinks.Console 5.0.0
- ? Serilog.Sinks.SQLite 6.0.0
- ? Serilog.Enrichers.Thread 3.1.0
- ? Serilog.Enrichers.Environment 2.3.0
- ? Serilog.Extensions.Logging 8.0.0
- ? Microsoft.Extensions.Configuration.Abstractions 9.0.0

## ������������

������ ������� ������ ��� ������ ?

```bash
dotnet build
# ������ ���������
```

## ��������� ���� (�����������)

### �������� File Sink ��� �������� �����
```bash
dotnet add package Serilog.Sinks.File
```

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "SQLite", "Args": {...} },
      {
    "Name": "File",
        "Args": {
  "path": "logs/log-.txt",
   "rollingInterval": "Day",
     "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

### �������� Seq ��� ����������������� �����������
```bash
dotnet add package Serilog.Sinks.Seq
```

```json
{
  "Serilog": {
    "WriteTo": [
    {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341"
     }
      }
    ]
  }
}
```

## ����������

�������� �� ���������������� ������ ��� Serilog ������� ���������:
- ? ��� ������� � ���� ����� ��������������
- ? ��������� �������� � ������������
- ? ��������� �������� ���������� ������������
- ? ������� ������ ������������
- ? ��������� �������� �������������
