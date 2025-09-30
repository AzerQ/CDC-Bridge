# SQL Server Agent в Dev Container

## Включение SQL Server Agent

SQL Server Agent теперь автоматически включается при запуске dev container благодаря следующим настройкам:

### 1. Переменная окружения в docker-compose.yml
```yaml
environment:
  MSSQL_AGENT_ENABLED: true
```

### 2. Автоматическая настройка в setup.sql
- Включение Agent XPs
- Запуск службы SQL Server Agent

## Управление SQL Server Agent

Используйте скрипт `manage_agent.sh` для управления SQL Server Agent:

```bash
# Проверить статус
./.devcontainer/mssql/manage_agent.sh status

# Запустить Agent
./.devcontainer/mssql/manage_agent.sh start

# Остановить Agent
./.devcontainer/mssql/manage_agent.sh stop

# Перезапустить Agent
./.devcontainer/mssql/manage_agent.sh restart
```

## Проверка работы CDC

После включения SQL Server Agent можно использовать Change Data Capture:

```bash
# Включить CDC на базе данных
sqlcmd -S localhost -U sa -P P@ssw0rd -i ./.devcontainer/mssql/enable_cdc.sql
```

## Важные замечания

1. **SQL Server Agent необходим для CDC** - Change Data Capture не будет работать без запущенного SQL Server Agent
2. **Автоматический запуск** - Agent настроен на автоматический запуск при создании контейнера
3. **Мониторинг** - Используйте скрипт проверки статуса для мониторинга состояния Agent

## Проверка статуса через SQL

```sql
-- Проверить статус служб
SELECT servicename, status, status_desc
FROM sys.dm_server_services
WHERE servicename LIKE '%Agent%';

-- Проверить конфигурацию Agent XPs
SELECT name, value, value_in_use
FROM sys.configurations
WHERE name = 'Agent XPs';
```