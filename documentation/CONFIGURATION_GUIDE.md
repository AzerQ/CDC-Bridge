# Руководство по конфигурации CDC Bridge

Система CDC Bridge использует гибкую многоуровневую систему конфигурации, позволяющую разделять системные настройки и бизнес-логику.

---

## 1. Источники конфигурации

Приложение собирает настройки из нескольких источников в следующем порядке приоритета (более поздние переопределяют более ранние):

1.  **appsettings.json** — базовые настройки приложения.
2.  **appsettings.{Environment}.json** — настройки для конкретной среды (Development, Production).
3.  **Файлы `*.customsettings.json`** — любые файлы с этим расширением, расположенные в директории приложения или поддиректориях. Они автоматически подгружаются при старте.
4.  **Переменные окружения** — поддерживаются стандартные правила .NET (например, `CdcBridge__ConfigurationPath` для переопределения пути к YAML).
5.  **cdc-settings.yaml** — основной файл бизнес-конфигурации (источники, приемники).

---

## 2. Системная конфигурация (JSON)

Эти настройки могут располагаться в `appsettings.json` или в любом `*.customsettings.json`.

### Секция CdcBridge
```json
"CdcBridge": {
  "ConfigurationPath": "cdc-settings.yaml", // Путь к YAML файлу
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
```

### Секция Persistence
```json
"Persistence": {
  "DbFilePath": "data/cdc_bridge.db"
}
```

---

## 3. Бизнес-конфигурация (YAML)

YAML файл (`cdc-settings.yaml`) определяет логику захвата и передачи данных.

### Динамические возможности (Препроцессинг)

CDC Bridge поддерживает специальные макросы для динамической подстановки значений:

#### 1. Макрос `Configuration("key")`
Позволяет подставлять значения из общего пула конфигурации (appsettings, customsettings, Environment Variables).
*   **Пример использования:**
    ```yaml
    connections:
      - name: MainDb
        type: MsSql
        parameters:
          # Значение будет взято из ConnectionStrings:Default в JSON или переменной окружения
          connectionString: Configuration("ConnectionStrings:Default")
    ```

#### 2. Макрос `IncludeFileContent("path")`
Вставляет содержимое текстового файла. Путь может быть относительным относительно YAML файла.
*   **Пример использования:**
    ```yaml
    transformers:
      - name: ComplexTransform
        type: JSONataTransformer
        parameters:
          transformation: IncludeFileContent("scripts/my_transform.jsonata")
    ```

---

## 4. Использование переменных окружения

Вы можете переопределить любую настройку через переменные окружения. Это особенно полезно при запуске в Docker.

*   `CdcBridge__ConfigurationPath` -> `CdcBridge:ConfigurationPath`
*   `Persistence__DbFilePath` -> `Persistence:DbFilePath`
*   `ConnectionStrings__Default` -> `ConnectionStrings:Default`

---

## 5. Полный пример структуры YAML

```yaml
connections:
  - name: ProductionDb
    type: MsSql
    parameters:
      connectionString: Configuration("ConnectionStrings:Production")

trackingInstances:
  - name: OrdersTracker
    connection: ProductionDb
    parameters:
      schemaName: sales
      tableName: Orders
      checkIntervalInSeconds: 5

filters:
  - name: StatusChangedToPaid
    type: StateTransitionFilter
    parameters:
      column: Status
      expression: "Pending -> Paid"

receivers:
  - name: BillingService
    type: HttpWebhook
    trackingInstance: OrdersTracker
    filter: StatusChangedToPaid
    parameters:
      webhookUrl: "https://billing.internal/api/orders/{{data.new.OrderId}}"
      urlIsTemplate: true
      httpMethod: POST
```
