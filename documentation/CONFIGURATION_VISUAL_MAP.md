# Визуальная карта конфигурации CDC Bridge

## Дерево структуры appsettings.json

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

## Легенда

| Символ | Значение |
|--------|----------|
| ?? | **REQUIRED** - обязательное поле на верхнем уровне |
| [REQUIRED] | Обязательное поле внутри объекта |
| [OPTIONAL] | Опциональное поле |
| [enum: ...] | Допустимые фиксированные значения |
| [integer: min-max] | Числовой тип с ограничениями |
| [string] | Строковый тип |
| [object] | Объект |
| [array] | Массив |
| {} | Объект с динамическими ключами |
| [] | Массив элементов |

## Минимально необходимая конфигурация

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

## Полная конфигурация с рекомендуемыми настройками

```json
{
  "Serilog": {          // Рекомендуется
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
  "Intervals": {      // Опционально
    "ChangesDelayIntervalInSeconds": 13
  },
  "ConnectionStrings": {        // Требуется для работы CDC
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
  "AllowedHosts": "*"  // Опционально
}
```

## Граф зависимостей

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

## Матрица типов и ограничений

| Поле | Тип | Мин | Макс | По умолчанию | Обязательно |
|------|-----|-----|------|--------------|-------------|
| `Serilog.MinimumLevel.Default` | enum | - | - | Information | ? |
| `Intervals.ChangesDelayIntervalInSeconds` | int | 1 | 3600 | 13 | ? (если Intervals есть) |
| `CdcBridge.ConfigurationPath` | string | - | - | cdc-settings.yaml | ? |
| `ReceiverWorker.PollingIntervalMs` | int | 100 | 60000 | 5000 | ? |
| `ReceiverWorker.BatchSize` | int | 1 | 1000 | 200 | ? |
| `CleanupWorker.CleanupIntervalHours` | int | 1 | 168 | 6 | ? |
| `CleanupWorker.BufferTimeToLiveHours` | int | 1 | 8760 | 24 | ? |
| `Persistence.DbFilePath` | string | - | - | data/cdc_bridge.db | ? |
| `ApiKeys.MasterPassword` | string | 8 chars | - | CHANGE_THIS... | ? |

## Чек-лист конфигурации для Production

### ? Обязательные действия

- [ ] **Изменить** `ApiKeys.MasterPassword` с дефолтного значения
- [ ] **Настроить** `ConnectionStrings.default` для вашей SQL Server БД
- [ ] **Создать** `cdc-settings.yaml` с вашими tracking instances и receivers
- [ ] **Указать** правильный путь в `CdcBridge.ConfigurationPath`
- [ ] **Настроить** `Persistence.DbFilePath` для хранения данных
- [ ] **Изменить** `AllowedHosts` с `*` на конкретные хосты

### ?? Рекомендуемые настройки

- [ ] **Настроить** уровни логирования в `Serilog.MinimumLevel.Override`
- [ ] **Добавить** дополнительные sinks (File, Seq) в `Serilog.WriteTo`
- [ ] **Оптимизировать** `ReceiverWorker.PollingIntervalMs` под нагрузку
- [ ] **Настроить** `ReceiverWorker.BatchSize` под размер изменений
- [ ] **Установить** `CleanupWorker.BufferTimeToLiveHours` под требования аудита

### ?? Безопасность

- [ ] **Не коммитить** пароли в Git (использовать User Secrets / переменные окружения)
- [ ] **Использовать** сильный `MasterPassword` (минимум 12 символов, спецсимволы)
- [ ] **Ограничить** `AllowedHosts` конкретными доменами
- [ ] **Включить** HTTPS в production
- [ ] **Ротировать** API ключи регулярно

## Связь с другими файлами

```
appsettings.json
      ?
      ?? указывает на ?? cdc-settings.yaml
      ?              ?
      ?        ?? TrackingInstances[]
      ?          ?? Receivers[]
      ?   ?? Filters[]
      ?  ?? Transformers[]
 ?
      ?? создает ?????????? data/cdc_bridge.db (Persistence)
      ?
      ?? создает ?????????? data/logs.db (Serilog SQLite)
```

## Выводы

? **Все поля покрыты JSON Schema** с:
- Полными описаниями на английском
- Валидацией типов
- Ограничениями значений (min/max)
- Enum для фиксированных значений
- Примерами использования
- Значениями по умолчанию

? **IntelliSense работает** для всех полей в Visual Studio и VS Code

? **Валидация происходит** в реальном времени при редактировании
