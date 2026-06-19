# CDC-Bridge — текущая архитектура приложения

## 1. Назначение документа

Документ описывает текущую архитектуру проекта **CDC-Bridge** по фактической структуре репозитория. Текущая версия сервиса решает задачу чтения изменений из SQL Server CDC, буферизации событий, фильтрации/трансформации и доставки в получатели, сейчас в первую очередь через webhook.

## 2. Текущая структура solution

```text
CDC-Bridge.sln
├── src
│   ├── CdcBridge.Core
│   ├── CdcBridge.Configuration
│   ├── CdcBridge.Application
│   ├── CdcBridge.Persistence
│   ├── CdcBridge.Service
│   ├── CdcBridge.Host
│   ├── CdcBridge.ApiClient
│   └── mssql-cdc/src/MsSqlCdc
├── tests
│   ├── CdcBridge.Application.Tests
│   ├── CdcBridge.Configuration.Tests
│   └── CdcBridge.Persistence.Tests
└── examples
    └── CdcBridge.Example.WebhookReceiver
```

| Проект | Ответственность |
|---|---|
| `CdcBridge.Core` | Базовые модели и интерфейсы: `ICdcSource`, `IReceiver`, `IFilter`, `ITransformer`. |
| `CdcBridge.Configuration` | YAML-модели, построение runtime-контекста, FluentValidation, cross-reference validation. |
| `CdcBridge.Application` | Реализации компонентов: SQL Server CDC source, JsonPath/StateTransition filters, JSONata transformer, Webhook receiver. |
| `CdcBridge.Persistence` | EF Core + SQLite state store: события, статусы доставки, checkpoints, API keys. |
| `CdcBridge.Service` | Runtime: orchestrator, source workers, receiver workers, cleanup worker, component factory. |
| `CdcBridge.Host` | ASP.NET Core host: API, Swagger, middleware, DI, Windows Service, запуск background services. |
| `CdcBridge.ApiClient` | Клиентская библиотека для API. |
| `MsSqlCdc` | Низкоуровневый слой работы с SQL Server CDC. |

## 3. Общая схема текущей архитектуры

### Mermaid

```mermaid
flowchart LR
    subgraph SourceDb["SQL Server Database"]
        Tables["Tracked tables"]
        CDC["SQL Server CDC functions/tables"]
    end

    subgraph Host["CdcBridge.Host"]
        API["Management API / Swagger"]
        Auth["API Key Middleware"]
        Orchestrator["CdcBridgeOrchestrator"]
    end

    subgraph Runtime["CdcBridge.Service"]
        SourceWorker["SourceWorker per TrackingInstance"]
        ReceiverWorker["ReceiverWorker per Receiver"]
        CleanupWorker["CleanupWorker"]
        Factory["ComponentFactory"]
    end

    subgraph Components["CdcBridge.Application"]
        SqlSource["SqlServerCdcSource"]
        JsonPath["JsonPathFilter"]
        StateTransition["StateTransitionFilter"]
        Jsonata["JSONataTransformer"]
        Webhook["WebhookReceiver"]
    end

    subgraph Storage["CdcBridge.Persistence"]
        SQLite["SQLite state DB"]
        Events["BufferedChangeEvents"]
        Statuses["ReceiverDeliveryStatuses"]
        Checkpoints["TrackingInstanceStates"]
    end

    External["External webhook endpoint"]

    Tables --> CDC --> SqlSource
    Host --> Orchestrator
    Orchestrator --> SourceWorker
    Orchestrator --> ReceiverWorker
    Orchestrator --> CleanupWorker
    SourceWorker --> SqlSource
    SourceWorker --> SQLite
    SQLite --> Events
    SQLite --> Statuses
    SQLite --> Checkpoints
    ReceiverWorker --> Factory
    Factory --> JsonPath
    Factory --> StateTransition
    Factory --> Jsonata
    Factory --> Webhook
    ReceiverWorker --> SQLite
    Webhook --> External
    API --> SQLite
    Auth --> API
```

### PlantUML

```plantuml
@startuml
skinparam componentStyle rectangle
skinparam shadowing false

database "SQL Server" as SqlServer {
  [Tracked Tables]
  [CDC Functions]
}

package "CdcBridge.Host" {
  [ASP.NET Core API] as Api
  [Swagger] as Swagger
  [API Key Middleware] as ApiKey
  [CdcBridgeOrchestrator] as Orchestrator
}

package "CdcBridge.Service" {
  [SourceWorker] as SourceWorker
  [ReceiverWorker] as ReceiverWorker
  [CleanupWorker] as CleanupWorker
  [ComponentFactory] as Factory
}

package "CdcBridge.Application" {
  [SqlServerCdcSource] as SqlSource
  [JsonPathFilter] as JsonPath
  [StateTransitionFilter] as StateFilter
  [JSONataTransformer] as Jsonata
  [WebhookReceiver] as Webhook
}

database "SQLite State DB" as SQLite {
  [BufferedChangeEvents]
  [ReceiverDeliveryStatuses]
  [TrackingInstanceStates]
}

cloud "External Webhook" as ExternalWebhook

[Tracked Tables] --> [CDC Functions]
[CDC Functions] --> SqlSource
ApiKey --> Api
Api --> SQLite
Orchestrator --> SourceWorker
Orchestrator --> ReceiverWorker
Orchestrator --> CleanupWorker
SourceWorker --> SqlSource
SourceWorker --> SQLite
ReceiverWorker --> Factory
Factory --> JsonPath
Factory --> StateFilter
Factory --> Jsonata
Factory --> Webhook
ReceiverWorker --> SQLite
Webhook --> ExternalWebhook
@enduml
```

## 4. Текущая модель конфигурации

Основная YAML-конфигурация содержит пять ключевых секций:

```yaml
connections: []
trackingInstances: []
receivers: []
filters: []
transformers: []
```

| Секция | Назначение |
|---|---|
| `connections` | Подключения к источникам данных. |
| `trackingInstances` | Описание отслеживаемых таблиц, колонок, схемы, подключения и интервала опроса. |
| `receivers` | Получатели событий. Сейчас основной сценарий — webhook. |
| `filters` | Фильтры событий: JsonPath, переходы состояний. |
| `transformers` | Преобразование события перед отправкой: JSONata. |

Пример:

```yaml
$schema: ./settings.schema.json

connections:
  - name: ExampleDbConnection
    type: SqlServer
    connectionString: Configuration("ConnectionStrings:default")
    active: true

trackingInstances:
  - name: EmployeeTracking
    sourceTable: employee
    sourceSchema: dbo
    capturedColumns:
      - id
      - first_name
      - last_name
      - email
    connection: ExampleDbConnection
    active: true
    checkIntervalInSeconds: 5

receivers:
  - name: EmployeeWebhook
    trackingInstance: EmployeeTracking
    type: WebhookReceiver
    parameters:
      webhookUrl: "http://localhost:5091/webhooks/employee"
      httpMethod: POST
      timeoutMs: 20000
```

## 5. Текущий поток обработки события

### Mermaid sequence diagram

```mermaid
sequenceDiagram
    autonumber
    participant SQL as SQL Server CDC
    participant SW as SourceWorker
    participant SRC as SqlServerCdcSource
    participant DB as SQLite Buffer
    participant RW as ReceiverWorker
    participant F as Filter
    participant T as Transformer
    participant R as WebhookReceiver
    participant EXT as External Webhook

    SW->>DB: GetLastProcessedRowLabelAsync()
    SW->>SRC: GetChanges(lastRowLabel)
    SRC->>SQL: Read CDC changes by LSN range
    SQL-->>SRC: CDC rows
    SRC-->>SW: TrackedChange[]
    SW->>DB: AddChangesToBufferAsync(changes)
    SW->>DB: SaveLastProcessedRowLabelAsync(lastRowLabel)

    RW->>DB: GetPendingChangesAsync(receiver, trackingInstance, batchSize)
    DB-->>RW: Pending buffered events

    loop for each event
        RW->>F: IsMatch(change)
        alt matched
            RW->>T: Transform(change)
            RW->>R: SendAsync(change)
            R->>EXT: HTTP request
            EXT-->>R: HTTP response
            R-->>RW: ReceiverProcessResult
            RW->>DB: UpdateChangeStatusAsync(success/failure)
        else filtered out
            RW->>DB: UpdateChangeStatusAsync(success, "Filtered out")
        end
    end
```

### PlantUML sequence diagram

```plantuml
@startuml
autonumber
participant "SQL Server CDC" as SQL
participant "SourceWorker" as SW
participant "SqlServerCdcSource" as SRC
database "SQLite Buffer" as DB
participant "ReceiverWorker" as RW
participant "Filter" as F
participant "Transformer" as T
participant "WebhookReceiver" as R
participant "External Webhook" as EXT

SW -> DB : GetLastProcessedRowLabelAsync()
SW -> SRC : GetChanges(lastRowLabel)
SRC -> SQL : Read CDC changes by LSN range
SQL --> SRC : CDC rows
SRC --> SW : TrackedChange[]
SW -> DB : AddChangesToBufferAsync(changes)
SW -> DB : SaveLastProcessedRowLabelAsync(lastRowLabel)

RW -> DB : GetPendingChangesAsync(receiver, trackingInstance, batchSize)
DB --> RW : Pending events

loop each event
  RW -> F : IsMatch(change)
  alt matched
    RW -> T : Transform(change)
    RW -> R : SendAsync(change)
    R -> EXT : HTTP request
    EXT --> R : HTTP response
    R --> RW : ReceiverProcessResult
    RW -> DB : UpdateChangeStatusAsync(success/failure)
  else filtered out
    RW -> DB : UpdateChangeStatusAsync(success, "Filtered out")
  end
end
@enduml
```

## 6. Runtime-компоненты

### `CdcBridgeOrchestrator`

Отвечает за:

- чтение runtime-конфигурации;
- инициализацию CDC на источнике;
- создание `SourceWorker` для каждого активного `trackingInstance`;
- создание `ReceiverWorker` для каждого receiver;
- управление остановкой воркеров через `CancellationToken`.

Текущая модель:

```text
1 active trackingInstance = 1 SourceWorker
1 receiver = 1 ReceiverWorker
```

### `SourceWorker`

```mermaid
flowchart TD
    Start["Start SourceWorker"] --> ReadCursor["Read last RowLabel"]
    ReadCursor --> ReadChanges["Read changes from ICdcSource"]
    ReadChanges --> HasChanges{"Has changes?"}
    HasChanges -- "yes" --> Buffer["Add changes to SQLite buffer"]
    Buffer --> SaveCursor["Save last RowLabel"]
    SaveCursor --> Delay["Delay checkIntervalInSeconds"]
    HasChanges -- "no" --> Delay
    Delay --> ReadCursor
```

### `ReceiverWorker`

```mermaid
flowchart TD
    Start["Start ReceiverWorker"] --> GetPending["Get pending changes"]
    GetPending --> Loop["For each change"]
    Loop --> RetryCheck{"Retry limit exceeded?"}
    RetryCheck -- "yes" --> MarkFailed["Mark failed"]
    RetryCheck -- "no" --> Filter{"Filter matched?"}
    Filter -- "no" --> MarkFiltered["Mark success: filtered out"]
    Filter -- "yes" --> Transform["Transform event"]
    Transform --> Send["Send to receiver"]
    Send --> Update["Update delivery status"]
    Update --> Loop
    MarkFailed --> Loop
    MarkFiltered --> Loop
    Loop --> Delay["Delay PollingIntervalMs"]
    Delay --> GetPending
```

### `ComponentFactory`

Текущая DI-based расширяемость:

```mermaid
flowchart LR
    Config["YAML type: WebhookReceiver"] --> Factory["ComponentFactory.GetInstance<IReceiver>()"]
    Factory --> DI["IServiceProvider.GetServices<IReceiver>()"]
    DI --> Match["Find by Name or class name"]
    Match --> Component["WebhookReceiver instance"]
```

## 7. Текущая модель хранения

### Таблицы

| Таблица | Назначение |
|---|---|
| `BufferedChangeEvents` | Буферизованные события изменений. |
| `ReceiverDeliveryStatuses` | Статусы доставки конкретного события конкретному receiver. |
| `TrackingInstanceStates` | Последний обработанный `RowLabel` по tracking instance. |
| `ApiKeys` | API-ключи для доступа к Management API. |

### Mermaid ER

```mermaid
erDiagram
    BufferedChangeEvents ||--o{ ReceiverDeliveryStatuses : has

    BufferedChangeEvents {
        guid Id PK
        string TrackingInstanceName
        string RowLabel
        json Change
        datetime BufferedAtUtc
    }

    ReceiverDeliveryStatuses {
        guid Id PK
        guid BufferedChangeEventId FK
        string ReceiverName
        int Status
        int AttemptCount
        datetime LastAttemptAtUtc
        string ErrorDescription
        long LastDeliveryTimeMs
        double AverageDeliveryTimeMs
    }

    TrackingInstanceStates {
        string TrackingInstanceName PK
        string LastProcessedRowLabel
        datetime UpdatedAtUtc
    }

    ApiKeys {
        int Id PK
        string Key
        string Name
        string Owner
        string Description
        int Permission
        bool IsActive
    }
```

### PlantUML ER

```plantuml
@startuml
hide circle
skinparam linetype ortho

entity "BufferedChangeEvents" as Events {
  * Id : Guid
  --
  TrackingInstanceName : string
  RowLabel : string
  Change : json
  BufferedAtUtc : DateTime
}

entity "ReceiverDeliveryStatuses" as Statuses {
  * Id : Guid
  --
  BufferedChangeEventId : Guid
  ReceiverName : string
  Status : DeliveryStatus
  AttemptCount : int
  LastAttemptAtUtc : DateTime?
  ErrorDescription : string?
  LastDeliveryTimeMs : long?
  AverageDeliveryTimeMs : double?
}

entity "TrackingInstanceStates" as Checkpoints {
  * TrackingInstanceName : string
  --
  LastProcessedRowLabel : string?
  UpdatedAtUtc : DateTime
}

entity "ApiKeys" as ApiKeys {
  * Id : int
  --
  Key : string
  Name : string
  Owner : string?
  Permission : int
  IsActive : bool
}

Events ||--o{ Statuses : contains
@enduml
```

## 8. Текущие сильные стороны

- Хорошее разделение на слои.
- YAML-конфигурация.
- Наличие runtime-контекста конфигурации.
- Персистентный буфер событий.
- Независимые статусы доставки по receiver.
- Базовые фильтры и трансформеры.
- Возможность расширения через интерфейсы и DI.
- ASP.NET Core Host API.
- Swagger.
- Docker-обвязка.
- Тестовые проекты.

## 9. Текущие ограничения и риски

| Ограничение | Риск |
|---|---|
| ReceiverWorker обрабатывает события последовательно | Низкий throughput при медленных receivers. |
| SQLite используется как основной runtime store | Ограничение по конкурентной записи и масштабированию. |
| Обновление статуса доставки по одному событию | Много мелких транзакций и I/O. |
| Cursor и buffer сохраняются раздельно | Возможны дубли или пропуски при аварии между операциями. |
| Нет route-модели | Сложно гибко настраивать много получателей и разные политики доставки. |
| Нет DLQ как отдельной сущности | Сложно эксплуатировать окончательно сломанные события. |
| Нет production observability | Требуются метрики, traces, структурированные логи и domain state. |
| Нет plugin manifest/schema contribution | Пользовательские компоненты сложно валидировать и документировать. |
| Нет PostgreSQL source/Kafka/RabbitMQ/gRPC/database sinks | Текущий функционал пока ограничен SQL Server CDC + webhook. |

## 10. Вывод

Текущая архитектура является хорошей MVP-основой. Она уже демонстрирует правильные идеи: источник, буфер, получатель, фильтр, трансформер, runtime orchestration и API.

Для production нужно развить проект в сторону:

- `CdcEvent` как единой внутренней модели;
- `SourcePosition` вместо простого `RowLabel`;
- route-driven delivery;
- batch-based sinks;
- асинхронного pipeline на `IAsyncEnumerable` и `Channel`;
- production state store;
- DLQ;
- redelivery jobs;
- observability;
- админской панели;
- плагинов через NuGet;
- YAML schema generation.
