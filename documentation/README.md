# CDC-Bridge

**Change Data Capture Bridge** - система для мониторинга изменений в базах данных и передачи этих изменений в различные внешние-приемники (Webhook, Kafka, RabbitMQ и др.).

## 🚀 Быстрый старт

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

## 🏗 Архитектура

### Основные проекты

- **CdcBridge.Host** - Входная точка приложения, объединяющая API и фоновые воркеры
- **CdcBridge.Core** - Базовые интерфейсы и модели
- **CdcBridge.Configuration** - Логика работы с YAML конфигурацией
- **CdcBridge.Persistence** - Слой хранения состояния на базе SQLite
- **CdcBridge.Application** - Реализации компонентов и внедрение зависимостей
- **CdcBridge.Service** - Оркестрация фоновых задач, воркеры
- **CdcBridge.ApiClient** - Клиентская библиотека для работы с API

## 🛠 Возможности

### API

- ✅ Контроль статуса воркеров
- ✅ Просмотр событий и их статусов доставки
- ✅ Просмотр логов
- ✅ Управление конфигурацией
- ✅ API Key аутентификация
- ✅ Swagger UI документация

### Background Services

- ✅ **SourceWorker** - Опрос источников данных (SQL Server CDC)
- ✅ **ReceiverWorker** - Обработка и доставка CDC событий
- ✅ **CleanupWorker** - Очистка устаревших событий

### Поддерживаемые источники данных

- ✅ MS SQL Server (CDC)
- ⏳ MySQL (Binary Log) - *в планах*
- ⏳ PostgreSQL (Logical Replication) - *в планах*

### Поддерживаемые приемники

- ✅ HTTP Webhook
- ⏳ Apache Kafka - *в планах*
- ⏳ RabbitMQ - *в планах*
- ⏳ Azure Service Bus - *в планах*
- ⏳ File - *в планах*

## ⚙️ Конфигурация

Приложение использует гибкую систему настроек через JSON (системные) и YAML (бизнес-логика).

Подробности в [Руководстве по конфигурации](CONFIGURATION_GUIDE.md).

## 📂 Структура

### Директории проекта

```
CDC-Bridge/
├── src/
│   ├── CdcBridge.Host/          # Хост-приложение (API + Workers)
│   ├── CdcBridge.Core/          # Ядро и абстракции
│   ├── CdcBridge.Configuration/ # Работа с YAML
│   ├── CdcBridge.Persistence/   # SQLite хранилище
│   ├── CdcBridge.Application/   # Реализации и DI
│   ├── CdcBridge.Service/       # Оркестратор и воркеры
│   └── CdcBridge.ApiClient/     # HTTP клиент
├── tests/                       # Модульные тесты
├── examples/                    # Примеры использования
└── docker-compose.host.yml      # Docker Compose конфигурация
```

## 📖 Документация

- [Описание компонентов](ComponentsDescription.md) — детальное описание внутренней архитектуры.
- [Руководство по конфигурации](CONFIGURATION_GUIDE.md) — как настроить источники, приемники и макросы.
- [Справочник REST API](API_REFERENCE.md) — описание эндпоинтов для мониторинга и управления.
- [Отчет о покрытии тестами](TEST_COVERAGE_SUMMARY.md) — текущее состояние тестирования.

## 🔐 Безопасность

API защищено с помощью API ключей. Подробности в [`API_KEY_AUTHENTICATION.md`](API_KEY_AUTHENTICATION.md).

⚠️ **Важно:** Обязательно смените мастер-пароль в production окружении через переменную окружения `ApiKeys__MasterPassword`.
