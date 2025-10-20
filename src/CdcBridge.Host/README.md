# CdcBridge.Host

Объединённое приложение CDC Bridge, включающее в себя:
- **API** для мониторинга и управления системой
- **Background Services** для обработки CDC событий

## Преимущества единого проекта

? **Единая кодовая база** - нет дублирования кода  
? **Упрощённое развёртывание** - один Docker-контейнер  
? **Общая инфраструктура** - единая конфигурация, логирование, база данных  
? **Меньше накладных расходов** - одно приложение вместо двух  

## Возможности

### API Endpoints

- `GET /api/metrics` - получение метрик системы
- `GET /api/events` - получение списка событий
- `GET /api/events/{id}` - получение детальной информации о событии
- `GET /api/logs` - получение логов системы
- `GET /api/configuration` - получение конфигурации системы

### Background Services

- **ReceiverWorker** - обработка и доставка CDC событий получателям
- **CleanupWorker** - очистка устаревших событий из буфера

## Запуск

### Локально

```bash
dotnet run --project src/CdcBridge.Host
```

### Docker

```bash
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
docker run -p 8080:8080 -v ./data:/app/data cdcbridge-host
```

### Docker Compose

```yaml
version: '3.8'
services:
  cdcbridge:
    build:
      context: .
      dockerfile: src/CdcBridge.Host/Dockerfile
    ports:
      - "8080:8080"
    volumes:
      - ./data:/app/data
      - ./cdc-settings.yaml:/app/cdc-settings.yaml
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

## Конфигурация

Конфигурация осуществляется через `appsettings.json` и переменные окружения:

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
    "Key": "YourSuperSecretKey",
    "Issuer": "CdcBridge.Host",
    "Audience": "CdcBridge.Client"
  }
}
```

## Swagger UI

В режиме разработки доступен Swagger UI: `http://localhost:8080/swagger`

## Аутентификация

API защищён JWT токенами. Для доступа к endpoints необходимо:

1. Получить JWT токен
2. Добавить заголовок: `Authorization: Bearer <token>`
