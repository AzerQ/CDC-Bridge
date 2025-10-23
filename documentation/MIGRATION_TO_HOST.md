# Миграция на CdcBridge.Host

## Что изменилось?

Проекты `CdcBridge.Api` и `CdcBridge.Worker` объединены в один проект `CdcBridge.Host`.

## Преимущества

- **Один проект** вместо двух
- **Единая конфигурация** - один `appsettings.json`
- **Одно развёртывание** - один Docker-контейнер
- **Нет дублирования кода** - общая инфраструктура

## Как мигрировать?

### 1. Docker Compose

**Было:**
```yaml
services:
  api:
    build: src/CdcBridge.Api
    ports:
      - "8080:8080"
  
  worker:
    build: src/CdcBridge.Worker
```

**Стало:**
```yaml
services:
  cdcbridge:
    build:
      context: .
      dockerfile: src/CdcBridge.Host/Dockerfile
    ports:
      - "8080:8080"
```

### 2. Конфигурация

Все настройки из обоих проектов теперь в одном `appsettings.json`:

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
  "Jwt": {
    "Key": "YourSecretKey",
    "Issuer": "CdcBridge.Host",
    "Audience": "CdcBridge.Client"
  }
}
```

### 3. Что делать со старыми проектами?

Проекты `CdcBridge.Api` и `CdcBridge.Worker` можно:
- Удалить из solution
- Оставить для совместимости (deprecated)
- Использовать `CdcBridge.Host` для новых развёртываний

## Функциональность

`CdcBridge.Host` включает в себя:

? **Все функции API:**
- Метрики
- События
- Логи
- Конфигурация
- Swagger UI
- JWT аутентификация

? **Все функции Worker:**
- ReceiverWorker (доставка событий)
- CleanupWorker (очистка буфера)
- Автоматические миграции БД

## Запуск

```bash
# Локально
cd src/CdcBridge.Host
dotnet run

# Docker
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
docker run -p 8080:8080 cdcbridge-host
```

API доступен на: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`
