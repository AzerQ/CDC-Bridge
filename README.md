# CDC Bridge — Система Захвата Изменений Данных

**CDC Bridge** — это расширяемая .NET-система для отслеживания изменений данных (Change Data Capture) в различных источниках, их обработки и доставки во внешние системы. Она спроектирована для работы в режиме 24/7, обеспечивая надежную и гарантированную доставку событий изменений.

## 🎯 Назначение

*   **Отслеживание изменений** в базах данных (SQL Server и др.) в режиме, близком к реальному времени.
*   **Буферизация** изменений для защиты от сбоев и перезапусков сервиса.
*   **Фильтрация** событий по гибким правилам перед обработкой.
*   **Трансформация** данных в формат, необходимый для системы-получателя.
*   **Гарантированная доставка** уведомлений через различные каналы (вебхуки и др.) с независимой обработкой для каждого получателя.

## 🏗️ Архитектура

Система построена на принципах модульности и расширяемости с использованием фоновых служб (.NET Worker Service).

```mermaid
graph TD
    subgraph "Источники Данных"
        style DB fill:#dae8fc,stroke:#6c8ebf
        DB[(SQL Server DB)]
    end

    subgraph "Ядро Системы (CdcBridge.Service)"
        style Orchestrator fill:#f8cecc,stroke:#b85450
        style Workers fill:#dae8fc,stroke:#6c8ebf
        
        Orchestrator(CdcBridgeOrchestrator) -->|запускает| SW[SourceWorker]
        Orchestrator -->|запускает| RW[ReceiverWorker]
        Orchestrator -->|запускает| CW[CleanupWorker]
    end
    
    subgraph "Компоненты (CdcBridge.Application)"
        style Components fill:#d5e8d4,stroke:#82b366
        
        CdcSource(ICdcSource: SqlServer)
        Filter(IFilter: JsonPath)
        Transformer(ITransformer: JSONata)
        Receiver(IReceiver: Webhook)
    end

    subgraph "Хранилище Состояния (CdcBridge.Persistence)"
        style Storage fill:#e1d5e7,stroke:#9673a6
        Storage[ICdcBridgeStorage<br/>(SQLite / EF Core)]
    end

    subgraph "Внешние Системы"
        style Webhook fill:#ffe6cc,stroke:#d79b00
        Webhook[API / Webhook Listener]
    end

    %% Потоки данных
    DB -- 1. CDC --> SW
    SW -- 2. Читает изменения --> CdcSource
    SW -- 3. Сохраняет в буфер --> Storage
    RW -- 4. Читает из буфера --> Storage
    RW -- 5. Применяет фильтр --> Filter
    RW -- 6. Трансформирует данные --> Transformer
    RW -- 7. Отправляет данные --> Receiver
    Receiver -- 8. HTTP POST --> Webhook
    CW -- 9. Периодически очищает --> Storage
```

### Основные компоненты

*   **Orchestrator**: Главный сервис, который читает конфигурацию и запускает по одному воркеру для каждого источника (`SourceWorker`) и получателя (`ReceiverWorker`).
*   **SourceWorker**: Опрашивает один источник данных (например, одну таблицу в SQL Server) и сохраняет все новые изменения в персистентный буфер (SQLite).
*   **ReceiverWorker**: Работает с одним получателем. Он забирает свою порцию данных из буфера, применяет к ним фильтры и трансформеры, а затем отправляет их. Каждый `ReceiverWorker` отслеживает свой собственный прогресс.
*   **CleanupWorker**: Фоновая задача, которая периодически удаляет из буфера старые события, успешно доставленные всем получателям.
*   **ICdcBridgeStorage**: Абстракция над хранилищем (SQLite), которая обеспечивает буферизацию и отслеживание состояния.
*   **Компоненты (`ICdcSource`, `IFilter` и др.)**: Реализации конкретных источников, фильтров, трансформеров и получателей, которые динамически загружаются на основе YAML-конфигурации.

## 📖 Руководство Пользователя (Конфигурация)

Для работы сервиса требуется два основных конфигурационных файла.

### 1. `appsettings.json`

Этот файл содержит настройки окружения для самого .NET-приложения.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information", // Рекомендуется 'Debug' для отладки
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "CdcBridge": {
    "ConfigurationPath": "cdc-settings.yaml", // Путь к главному YAML-файлу
    "CleanupIntervalHours": 1 // Как часто запускать очистку буфера
  },
  "Persistence": {
    // Путь к файлу базы данных состояния (буфера)
    "SqliteDbPath": "data/cdc_bridge.db" 
  },
  "ConnectionStrings": {
    // Секция для хранения строк подключения и других секретов
    "DefaultConnection": "Server=localhost;Database=...;User Id=...;Password=..."
  }
}
```

### 2. `cdc-settings.yaml`

Это главный файл, описывающий логику работы CDC Bridge. Он поддерживает два специальных макроса:
*   `Configuration("Key:Path")`: Вставляет значение из `appsettings.json`.
*   `IncludeFileContent("path/to/file.txt")`: Вставляет содержимое файла как многострочную строку.

```yaml
# Список подключений к базам данных
connections:
  - name: ExampleDbConnection
    type: SqlServer
    # Используем макрос для безопасного получения строки подключения
    connectionString: Configuration("ConnectionStrings:DefaultConnection")
    active: true

# Экземпляры отслеживания (какие таблицы слушать)
trackingInstances:
  - name: EmployeeTracking
    sourceTable: employee
    sourceSchema: dbo
    connection: ExampleDbConnection
    active: true
    checkIntervalInSeconds: 5

# Фильтры для отбора событий
filters:
  - name: ActiveUsersFilter
    trackingInstance: EmployeeTracking
    type: JsonPathFilter
    parameters:
      # Фильтруем только события, где поле 'is_active' стало false
      expression: "$[?(@.data.new.is_active == false)]"

# Трансформеры для преобразования данных
transformers:
  - name: AnalyticsTransformer
    trackingInstance: EmployeeTracking
    type: JSONataTransformer
    parameters:
      # Используем макрос для загрузки сложного JSONata-выражения из файла
      transformation: IncludeFileContent("transformers/analytics-format.jsonata")

# Получатели (куда отправлять данные)
receivers:
  - name: EmployeeWebhook
    trackingInstance: EmployeeTracking
    # filter: ActiveUsersFilter           # Можно применить фильтр
    # transformer: AnalyticsTransformer # Можно применить трансформер
    type: WebhookReceiver
    parameters:
      webhookUrl: "http://localhost:5123/webhooks/employee"
      httpMethod: POST
```

## 👨‍💻 Руководство по Разработке

### Настройка окружения

1.  **.NET 8 SDK** или выше.
2.  **IDE**: Visual Studio 2022, JetBrains Rider или VS Code.
3.  **База данных**: SQL Server (можно запустить в Docker).
4.  **Producer (опционально)**: Проект `CdcBridge.Example.WorkerService` для генерации тестовых изменений.
5.  **Listener (опционально)**: Проект `WebhookListener` для приема и отображения отправленных событий.

### Структура проекта

*   `CdcBridge.Core`: Абстракции и основные модели.
*   `CdcBridge.Configuration`: Логика работы с YAML-конфигурацией.
*   `CdcBridge.Persistence`: Слой хранения состояния (буфер на SQLite).
*   `CdcBridge.Application`: Стандартные реализации компонентов (`SqlServerCdcSource`, `WebhookReceiver` и др.) и DI-конфигурация.
*   `CdcBridge.Service`: Ядро системы (оркестратор и воркеры).
*   `CdcBridge.Worker`: Исполняемый проект .NET Worker Service ("коробка").

### Расширение системы

Система спроектирована для легкого добавления новых компонентов.

#### Пример: Добавление нового Получателя (IReceiver)

1.  **Создайте класс**, реализующий `IReceiver`, в проекте `CdcBridge.Application` (или в вашем собственном проекте).
    ```csharp
    public class MyKafkaReceiver : IReceiver
    {
        public string Name => nameof(MyKafkaReceiver);
        
        public async Task<ReceiverProcessResult> SendAsync(TrackedChange trackedChange, JsonElement parameters)
        {
            // Ваша логика отправки в Kafka...
            return new ReceiverProcessResult { Status = ReceiverProcessStatus.Success };
        }
    }
    ```
2.  **Зарегистрируйте его в DI**. Откройте `CdcBridge.Application/DI/CdcBridgeServiceCollectionExtensions.cs` и добавьте в метод `AddCdcBridgeApplicationComponents`:
    ```csharp
    // ...
    services.AddTransient<MyKafkaReceiver>();
    services.AddTransient<IReceiver, MyKafkaReceiver>(s => s.GetRequiredService<MyKafkaReceiver>());
    ```
3.  **Используйте в YAML**:
    ```yaml
    receivers:
      - name: KafkaChannel
        trackingInstance: EmployeeTracking
        type: MyKafkaReceiver # <-- Ваша новая реализация
        parameters:
          topic: "user-changes"
          bootstrapServers: "kafka:9092"
    ```
Аналогичный процесс применяется для добавления `ICdcSource`, `IFilter` и `ITransformer`.

## 🚀 Руководство по Развертыванию

### Сборка приложения

Для развертывания используется проект `CdcBridge.Worker`. Соберите его для нужной платформы:

```bash
# Для Windows x64
dotnet publish CdcBridge.Worker -c Release -r win-x64 --self-contained true

# Для Linux x64
dotnet publish CdcBridge.Worker -c Release -r linux-x64 --self-contained true
```

Команда создаст папку `publish` со всеми необходимыми файлами.

### Конфигурация

Перед запуском скопируйте в папку `publish` ваши файлы `appsettings.json` и `cdc-settings.yaml` и настройте их для продакшн-окружения.

### Развертывание в Windows (как служба)

1.  Скопируйте содержимое папки `publish` на целевой сервер (например, в `C:\CdcBridge`).
2.  Откройте PowerShell от имени администратора.
3.  Создайте службу Windows с помощью утилиты `sc`:
    ```powershell
    sc.exe create CdcBridge binPath="C:\CdcBridge\CdcBridge.Worker.exe" start=auto
    ```
4.  Запустите службу:
    ```powershell
    sc.exe start CdcBridge
    ```
    Для остановки: `sc.exe stop CdcBridge`. Для удаления: `sc.exe delete CdcBridge`.

### Развертывание в Linux (как systemd сервис)

1.  Скопируйте содержимое папки `publish` на целевой сервер (например, в `/opt/cdc-bridge`).
2.  Создайте файл сервиса для `systemd`:
    ```bash
    sudo nano /etc/systemd/system/cdc-bridge.service
    ```
3.  Вставьте в него следующее содержимое, поменяв пути при необходимости:
    ```ini
    [Unit]
    Description=CDC Bridge Service

    [Service]
    # Путь к исполняемому файлу
    ExecStart=/opt/cdc-bridge/CdcBridge.Worker
    # Рабочая директория
    WorkingDirectory=/opt/cdc-bridge
    User=www-data # Рекомендуется запускать от непривилегированного пользователя
    Restart=always
    RestartSec=10
    SyslogIdentifier=cdc-bridge
    Environment=ASPNETCORE_ENVIRONMENT=Production

    [Install]
    WantedBy=multi-user.target
    ```
4.  Перезагрузите конфигурацию `systemd`, включите и запустите сервис:
    ```bash
    sudo systemctl daemon-reload
    sudo systemctl enable cdc-bridge.service
    sudo systemctl start cdc-bridge.service
    ```
    Для проверки статуса: `sudo systemctl status cdc-bridge`. Для просмотра логов: `sudo journalctl -u cdc-bridge -f`.

### Развертывание в Docker

1.  Создайте `Dockerfile` в папке проекта `CdcBridge.Worker`:
    ```dockerfile
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    WORKDIR "/src/CdcBridge.Worker"
    RUN dotnet publish -c Release -o /app/publish --no-restore

    FROM mcr.microsoft.com/dotnet/aspnet:8.0
    WORKDIR /app
    COPY --from=build /app/publish .
    ENTRYPOINT ["dotnet", "CdcBridge.Worker.dll"]
    ```
2.  Соберите образ:
    ```bash
    docker build -t cdc-bridge .
    ```
3.  Запустите контейнер, пробросив конфигурационные файлы как volume:
    ```bash
    docker run -d --name cdc-bridge-instance \
      -v /path/to/your/configs/appsettings.json:/app/appsettings.json \
      -v /path/to/your/configs/cdc-settings.yaml:/app/cdc-settings.yaml \
      cdc-bridge
    ```