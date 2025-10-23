# Архивные проекты

## CdcBridge.Api и CdcBridge.Worker - УСТАРЕЛИ

**Дата архивации:** 20 октября 2025  
**Причина:** Объединены в единый проект `CdcBridge.Host`

### Что было сделано

1. **CdcBridge.Api** и **CdcBridge.Worker** объединены в **CdcBridge.Host**
2. Все контроллеры, сервисы, DTOs скопированы с обновлёнными namespace'ами
3. Конфигурации объединены в единый `appsettings.json`
4. Создан единый Dockerfile и docker-compose файл

### Что было в проектах

#### CdcBridge.Api
- Controllers: MetricsController, EventsController, LogsController, ConfigurationController
- Services: MetricsService, EventsService, LogsService
- DTOs: MetricsDto, EventDto, LogDto
- JWT аутентификация
- Swagger UI

#### CdcBridge.Worker
- Background Services (через AddCdcBridge):
  - ReceiverWorker
  - CleanupWorker
- Database Migrations
- Автоматическое применение миграций при старте

### Где теперь находится функциональность

Вся функциональность доступна в проекте **src/CdcBridge.Host/**

### Удалённые файлы

Проекты были удалены после проверки, что вся функциональность перенесена в CdcBridge.Host:
- `src/CdcBridge.Api/` - удалён
- `src/CdcBridge.Worker/` - удалён

### Как восстановить

Если по какой-то причине понадобится восстановить старые проекты:
```bash
git checkout <commit_before_deletion> -- src/CdcBridge.Api
git checkout <commit_before_deletion> -- src/CdcBridge.Worker
```

Последний коммит с этими проектами: `[будет добавлен после коммита]`
