# ���������� ����� ������������ CDC Bridge

## ������ ��������� appsettings.json

```
?? appsettings.json
?
??? ?? Serilog        [OPTIONAL]
?   ??? ??? MinimumLevel
?   ?   ??? Default: "Information"               [enum: Verbose|Debug|Information|Warning|Error|Fatal]
?   ? ??? Override: {} [object with namespace keys]
?   ?       ??? Microsoft: "Warning"
?   ?     ??? Microsoft.EntityFrameworkCore: "Warning"
?   ?       ??? System: "Warning"
?   ??? ??? Enrich: []          [array of enrichers]
?   ?   ??? "FromLogContext"
?   ?   ??? "WithThreadId"
?   ?   ??? "WithMachineName"
?   ??? ?? WriteTo: []          [array of sinks]
???? [0] Console
?       ?   ??? Name: "Console"[REQUIRED]
?       ?   ??? Args:
?       ?   ??? outputTemplate: "[{Timestamp}...]"
?       ??? [1] SQLite
?           ??? Name: "SQLite"      [REQUIRED]
?           ??? Args:
?       ??? sqliteDbPath: "data/logs.db"
?   ??? tableName: "Logs"
?  ??? restrictedToMinimumLevel: "Information"
?    ??? storeTimestampInUtc: true
?
??? ?? Logging (Legacy ASP.NET Core)    [OPTIONAL]
?   ??? LogLevel: {}
?       ??? Default: "Debug"
?   ??? System: "Information"
?       ??? Microsoft: "Information"
?
??? ?? Intervals    [OPTIONAL, but field is REQUIRED if present]
?   ??? ChangesDelayIntervalInSeconds: 13        [REQUIRED] [integer: 1-3600]
?
??? ?? ConnectionStrings       [OPTIONAL]
?   ??? default: "Data Source=..."              [string - SQL Server connection]
?   ??? [custom]: "..."       [additional connections]
?
??? ?? CdcBridge                [REQUIRED] ??
?   ??? ConfigurationPath: "cdc-settings.yaml"   [REQUIRED] [string]
?   ??? WorkersConfiguration:           [REQUIRED] [object]
?   ??? ReceiverWorker: [REQUIRED] [object]
?       ?   ??? PollingIntervalMs: 5000          [REQUIRED] [integer: 100-60000]
? ?   ??? BatchSize: 200    [REQUIRED] [integer: 1-1000]
?       ??? CleanupWorker:             [REQUIRED] [object]
?           ??? CleanupIntervalHours: 6      [REQUIRED] [integer: 1-168]
?       ??? BufferTimeToLiveHours: 24        [REQUIRED] [integer: 1-8760]
?
??? ?? Persistence               [REQUIRED] ??
?   ??? DbFilePath: "data/cdc_bridge.db"    [REQUIRED] [string]
?
??? ?? ApiKeys        [REQUIRED] ??
?   ??? MasterPassword: "CHANGE_THIS..."         [REQUIRED] [string, minLength: 8]
?
??? ?? AllowedHosts: "*"  [OPTIONAL] [string]
```

## �������

| ������ | �������� |
|--------|----------|
| ?? | **REQUIRED** - ������������ ���� �� ������� ������ |
| [REQUIRED] | ������������ ���� ������ ������� |
| [OPTIONAL] | ������������ ���� |
| [enum: ...] | ���������� ������������� �������� |
| [integer: min-max] | �������� ��� � ������������� |
| [string] | ��������� ��� |
| [object] | ������ |
| [array] | ������ |
| {} | ������ � ������������� ������� |
| [] | ������ ��������� |

## ���������� ����������� ������������

```json
{
  "CdcBridge": {             // ?? REQUIRED
    "ConfigurationPath": "cdc-settings.yaml",  // REQUIRED
    "WorkersConfiguration": {                 // REQUIRED
      "ReceiverWorker": {        // REQUIRED
        "PollingIntervalMs": 5000,            // REQUIRED
        "BatchSize": 200        // REQUIRED
      },
      "CleanupWorker": {    // REQUIRED
    "CleanupIntervalHours": 6,     // REQUIRED
        "BufferTimeToLiveHours": 24         // REQUIRED
      }
    }
  },
  "Persistence": {          // ?? REQUIRED
    "DbFilePath": "data/cdc_bridge.db"    // REQUIRED
  },
  "ApiKeys": {        // ?? REQUIRED
    "MasterPassword": "YourSecurePassword123!"    // REQUIRED (min 8 chars)
  }
}
```

## ������ ������������ � �������������� �����������

```json
{
  "Serilog": {          // �������������
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
  "System": "Warning"
      }
    },
    "Enrich": ["FromLogContext", "WithThreadId", "WithMachineName"],
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
  },
  "Intervals": {      // �����������
    "ChangesDelayIntervalInSeconds": 13
  },
  "ConnectionStrings": {        // ��������� ��� ������ CDC
"default": "Data Source=server;Initial Catalog=db;..."
  },
  "CdcBridge": {         // ?? REQUIRED
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
  "Persistence": {  // ?? REQUIRED
    "DbFilePath": "data/cdc_bridge.db"
  },
  "ApiKeys": {    // ?? REQUIRED
    "MasterPassword": "YourSecurePassword123!"
  },
  "AllowedHosts": "*"  // �����������
}
```

## ���� ������������

```
???????????????????????????????????????????????????????????????
?                appsettings.json  ?
???????????????????????????????????????????????????????????????
 ?
         ??????????????????????????????????????????????????????
         ?             ?             ?   ?        ?
         ?     ?   ?    ?          ?
    ??????????   ???????????   ????????????  ???????????? ??????????
    ?Serilog ?   ?CdcBridge?   ?Persistence?  ? ApiKeys  ? ?Intervals?
    ?Optional?   ?REQUIRED ?   ? REQUIRED  ?  ? REQUIRED ? ?Optional?
  ??????????   ???????????   ????????????  ???????????? ??????????
           ?
      ???????????????????????????
         ?    ?
       ?       ?
????????????????????    ?????????????????????
?ConfigurationPath ?    ?WorkersConfiguration?
?    REQUIRED      ?    ? REQUIRED ?
?  "cdc-settings"  ?    ??????????????????????
????????????????????              ?
      ???????????????????
    ?           ?
            ?     ?
     ????????????????? ????????????????
                 ?ReceiverWorker ? ?CleanupWorker ?
 ?   REQUIRED    ? ?   REQUIRED   ?
         ????????????????? ????????????????
          ?     ?
     ???????????????????? ??????????????????
   ?       ? ?        ?
              ?       ? ?             ?
        PollingIntervalMs  BatchSize  CleanupIntervalHours  BufferTimeToLiveHours
           REQUIRED        REQUIRED      REQUIRED    REQUIRED
          100-60000ms       1-1000   1-168h     1-8760h
```

## ������� ����� � �����������

| ���� | ��� | ��� | ���� | �� ��������� | ����������� |
|------|-----|-----|------|--------------|-------------|
| `Serilog.MinimumLevel.Default` | enum | - | - | Information | ? |
| `Intervals.ChangesDelayIntervalInSeconds` | int | 1 | 3600 | 13 | ? (���� Intervals ����) |
| `CdcBridge.ConfigurationPath` | string | - | - | cdc-settings.yaml | ? |
| `ReceiverWorker.PollingIntervalMs` | int | 100 | 60000 | 5000 | ? |
| `ReceiverWorker.BatchSize` | int | 1 | 1000 | 200 | ? |
| `CleanupWorker.CleanupIntervalHours` | int | 1 | 168 | 6 | ? |
| `CleanupWorker.BufferTimeToLiveHours` | int | 1 | 8760 | 24 | ? |
| `Persistence.DbFilePath` | string | - | - | data/cdc_bridge.db | ? |
| `ApiKeys.MasterPassword` | string | 8 chars | - | CHANGE_THIS... | ? |

## ���-���� ������������ ��� Production

### ? ������������ ��������

- [ ] **��������** `ApiKeys.MasterPassword` � ���������� ��������
- [ ] **���������** `ConnectionStrings.default` ��� ����� SQL Server ��
- [ ] **�������** `cdc-settings.yaml` � ������ tracking instances � receivers
- [ ] **�������** ���������� ���� � `CdcBridge.ConfigurationPath`
- [ ] **���������** `Persistence.DbFilePath` ��� �������� ������
- [ ] **��������** `AllowedHosts` � `*` �� ���������� �����

### ?? ������������� ���������

- [ ] **���������** ������ ����������� � `Serilog.MinimumLevel.Override`
- [ ] **��������** �������������� sinks (File, Seq) � `Serilog.WriteTo`
- [ ] **��������������** `ReceiverWorker.PollingIntervalMs` ��� ��������
- [ ] **���������** `ReceiverWorker.BatchSize` ��� ������ ���������
- [ ] **����������** `CleanupWorker.BufferTimeToLiveHours` ��� ���������� ������

### ?? ������������

- [ ] **�� ���������** ������ � Git (������������ User Secrets / ���������� ���������)
- [ ] **������������** ������� `MasterPassword` (������� 12 ��������, �����������)
- [ ] **����������** `AllowedHosts` ����������� ��������
- [ ] **��������** HTTPS � production
- [ ] **����������** API ����� ���������

## ����� � ������� �������

```
appsettings.json
      ?
      ?? ��������� �� ?? cdc-settings.yaml
      ?              ?
      ?        ?? TrackingInstances[]
      ?          ?? Receivers[]
      ?   ?? Filters[]
      ?  ?? Transformers[]
 ?
      ?? ������� ?????????? data/cdc_bridge.db (Persistence)
      ?
      ?? ������� ?????????? data/logs.db (Serilog SQLite)
```

## ������

? **��� ���� ������� JSON Schema** �:
- ������� ���������� �� ����������
- ���������� �����
- ������������� �������� (min/max)
- Enum ��� ������������� ��������
- ��������� �������������
- ���������� �� ���������

? **IntelliSense ��������** ��� ���� ����� � Visual Studio � VS Code

? **��������� ����������** � �������� ������� ��� ��������������
