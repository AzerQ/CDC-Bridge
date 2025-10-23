# ������������ ������������������ ����������� Serilog

## �����

������� ����������� � CDC Bridge ���������� Serilog � ������������� ����� `appsettings.json`. ��� ��������� ����������� �������� �� ������������, ��� ��������� ����� ��������� �������� ����������� ��� �������������� ����.

## �������� �� �������� � ������������

### ���� (�������):
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

### ����� (������������):
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

## ��������� ������������

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

## ��������� ������������

### MinimumLevel

#### Default
����������� ������� ����������� �� ��������� ��� ���� ����������.

**��������� ��������:**
- `Verbose` - ������������ �����������
- `Debug` - ���������� ����������
- `Information` - �������������� ���������
- `Warning` - ��������������
- `Error` - ������
- `Fatal` - ����������� ������

**������������:**
- **Production**: `Information`
- **Development**: `Debug`

#### Override
��������������� ������� ��� ���������� ����������� ����.

```json
"Override": {
  "Microsoft": "Warning",
  "Microsoft.EntityFrameworkCore": "Warning",
  "System": "Warning"
}
```

### Enrich
���������� ����� �������������� �����������.

**��������� enrichers:**
- `FromLogContext` - �������� ����������
- `WithThreadId` - ID ������
- `WithMachineName` - ��� ������
- `WithEnvironmentName` - ��������� (Development, Production)
- `WithProcessId` - ID ��������
- `WithProcessName` - ��� ��������

### WriteTo
���������� ��� ������ ����� (sinks).

#### Console Sink
����� ����� � �������.

```json
{
  "Name": "Console",
  "Args": {
    "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
  }
}
```

**��������� �������:**
- `{Timestamp:yyyy-MM-dd HH:mm:ss}` - ��������� �����
- `{Level:u3}` - ������� ����������� (����������, 3 �������)
- `{Message:lj}` - ��������� (literal JSON)
- `{Properties:j}` - �������������� �������� (JSON)
- `{NewLine}` - ������� ������
- `{Exception}` - ���������� �� ����������

#### SQLite Sink
���������� ����� � ���� ������ SQLite.

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

**���������:**
- `sqliteDbPath` - ���� � ����� ���� ������
- `tableName` - ��� ������� (������ "Logs")
- `restrictedToMinimumLevel` - ����������� ������� ��� ����� sink
- `storeTimestampInUtc` - ������� ����� � UTC

## ������� ���������

### ��������� ���������� ����������� EF Core
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

### ����������� ������ ������ � Production
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

### ���������� File Sink
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

��� ������������� File sink ����� ���������� �����:
```bash
dotnet add package Serilog.Sinks.File
```

## ���������� ���������

����� �������������� ��������� ����� ���������� ���������:

```bash
# �������� ������� �����������
export Serilog__MinimumLevel__Default=Debug

# �������� ���� � ���� ������
export Serilog__WriteTo__1__Args__sqliteDbPath=/var/logs/cdc-bridge.db
```

� Docker Compose:
```yaml
environment:
  - Serilog__MinimumLevel__Default=Information
  - Serilog__WriteTo__1__Args__sqliteDbPath=/app/data/logs.db
```

## ������������ ����������������� �������

### 1. **��������**
- ? ��������� ������� ��� ��������������
- ? ������ ��������� ��� ������ ���������
- ? ������ ���������� ����� sinks

### 2. **����������**
- ? ���������������� ������������
- ? ��������������� ����� ���������� ���������
- ? ��������� ���������������� ����������� ASP.NET Core

### 3. **���������**
- ? �� ����� �������� ��� ��� ��������� �����������
- ? ���������� ������������ � ������� ��������
- ? ������� ��������� ���������� ����������� ��� �������

## ��������� ������� Logs � SQLite

Serilog.Sinks.SQLite ������������� ������� ������� �� ��������� ����������:

| ���� | ��� | �������� |
|------|-----|----------|
| Id | INTEGER PRIMARY KEY | ���������������� ������������� |
| Timestamp | TEXT | ��������� ����� ���� (UTC) |
| Level | TEXT | ������� ����������� |
| Exception | TEXT | ���������� �� ���������� |
| RenderedMessage | TEXT | ����������������� ��������� |
| Properties | TEXT | �������������� �������� (JSON) |

## ���������� �����

���� �������� ����� API:

```http
GET /api/Logs?page=1&pageSize=50&level=Error
Authorization: X-API-Key: your-api-key
```

���������:
- `page` - ����� ��������
- `pageSize` - ������ ��������
- `level` - ������ �� ������ (Debug, Information, Warning, Error, Fatal)
- `messageSearch` - ����� �� ������ ���������
- `fromDate` - ��������� ����
- `toDate` - �������� ����

## ���������� ���������

### ���� �� ������������ � SQLite
1. ��������� ����� ������� � ���������� `data/`
2. ���������, ��� `restrictedToMinimumLevel` �� ���� �������� ������
3. ��������� ������� ������ `Serilog.Sinks.SQLite`

### ������� ����� �����
1. �������� `MinimumLevel.Default` �� `Warning` ��� `Error`
2. �������� ��������������� ��� ������ ����������� ����
3. ��������� `restrictedToMinimumLevel` ��� SQLite sink

### ���� �� ���������� � �������
1. ��������� ������� Console sink � `WriteTo`
2. ���������, ��� ������� ����������� ���������� ������

## ��������� ������

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

## ����������

�������� �� ���������������� ������ ��� Serilog ������������:
- ? ������ ���������� �������� �����������
- ? ������ ��������� ��� ������ ���������
- ? ���������� ������������� � ��������������
- ? ��������� ���������� ���������
- ? ������������ ������ ��������� .NET
