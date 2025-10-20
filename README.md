# CDC-Bridge

**Change Data Capture Bridge** - система для отслеживания изменений в базах данных и передачи этих изменений в различные системы-получатели (Webhook, Kafka, RabbitMQ и др.).

## ?? Быстрый старт

### Используя Docker Compose

```bash
docker-compose -f docker-compose.host.yml up -d
```

Приложение будет доступно по адресу:
- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`

### Локально

```bash
dotnet run --project src/CdcBridge.Host
```

## ?? Архитектура

### Основные проекты

- **CdcBridge.Host** - главное приложение, объединяющее API и фоновые сервисы
- **CdcBridge.Core** - базовые интерфейсы и модели
- **CdcBridge.Configuration** - работа с YAML конфигурацией
- **CdcBridge.Persistence** - работа с SQLite базой данных
- **CdcBridge.Application** - бизнес-логика и dependency injection
- **CdcBridge.Service** - реализации источников данных, фильтров, трансформеров и получателей
- **CdcBridge.Logging** - структурированное логирование
- **CdcBridge.ApiClient** - клиент для работы с API

### Устаревшие проекты (удалены)

- ~~CdcBridge.Api~~ ? объединён в **CdcBridge.Host**
- ~~CdcBridge.Worker~~ ? объединён в **CdcBridge.Host**

## ?? Возможности

### API

- ? Просмотр метрик системы
- ? Просмотр событий и их статусов доставки
- ? Просмотр логов
- ? Просмотр конфигурации
- ? JWT аутентификация
- ? Swagger UI документация

### Background Services

- ? **ReceiverWorker** - обработка и доставка CDC событий
- ? **CleanupWorker** - очистка устаревших событий

### Поддерживаемые источники данных

- ? MS SQL Server (Change Tracking)
- ? MySQL (Binary Log)
- ? PostgreSQL (Logical Replication)

### Поддерживаемые получатели

- ? HTTP Webhook
- ? Apache Kafka
- ? RabbitMQ
- ? Azure Service Bus
- ? File (локальная файловая система)

## ?? Конфигурация

Пример `cdc-settings.yaml`:

```yaml
connections:
  - name: MyDatabase
    type: MsSql
    parameters:
      connectionString: "Server=localhost;Database=MyDB;User Id=sa;Password=xxx"

trackingInstances:
  - name: UsersTable
    connection: MyDatabase
    parameters:
      tableName: Users
      schemaName: dbo

receivers:
  - name: WebhookReceiver
    type: HttpWebhook
    trackingInstance: UsersTable
    parameters:
      url: "https://example.com/webhook"
      method: POST
```

## ??? Разработка

### Структура решения

```
CDC-Bridge/
??? src/
?   ??? CdcBridge.Host/          # Главное приложение (API + Workers)
?   ??? CdcBridge.Core/          # Базовые интерфейсы
?   ??? CdcBridge.Configuration/ # YAML конфигурация
?   ??? CdcBridge.Persistence/   # SQLite база данных
?   ??? CdcBridge.Application/   # DI и бизнес-логика
?   ??? CdcBridge.Service/       # Реализации компонентов
?   ??? CdcBridge.Logging/       # Логирование
?   ??? CdcBridge.ApiClient/     # HTTP клиент
??? tests/                       # Юнит-тесты
??? examples/                    # Примеры использования
??? docker-compose.host.yml      # Docker Compose конфигурация
```

### Запуск тестов

```bash
dotnet test
```

### Сборка

```bash
dotnet build CDC-Bridge.sln
```

## ?? Docker

### Сборка образа

```bash
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
```

### Запуск контейнера

```bash
docker run -d \
  -p 8080:8080 \
  -v ./data:/app/data \
  -v ./cdc-settings.yaml:/app/cdc-settings.yaml \
  -e Jwt__Key="YourSecretKey" \
  cdcbridge-host
```

## ?? Документация

- [Миграция на CdcBridge.Host](MIGRATION_TO_HOST.md)
- [Архивные проекты](ARCHIVED_PROJECTS.md)
- [Покрытие тестами](TEST_COVERAGE_SUMMARY.md)

## ?? Безопасность

### JWT Аутентификация

API защищён JWT токенами. Настройка в `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CdcBridge.Host",
    "Audience": "CdcBridge.Client",
    "ExpirationMinutes": 60
  }
}
```

?? **Важно:** Замените ключ на свой собственный в production окружении!

## ?? Мониторинг

### Метрики

Доступны через API endpoint `GET /api/metrics`:
- Количество событий в буфере
- Статусы доставки (pending, success, failed)
- Среднее время доставки
- Метрики по каждому получателю

### Логи

- Структурированные логи в SQLite
- Доступ через API: `GET /api/logs`
- Фильтрация по уровню, времени, тексту

## ?? Вклад в проект

Приветствуются pull requests! Для больших изменений сначала откройте issue для обсуждения.

## ?? Лицензия

[Укажите вашу лицензию]

## ?? Контакты

[Укажите контактную информацию]
