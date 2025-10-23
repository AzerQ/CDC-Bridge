# API Key Authentication Guide

## Обзор

CDC Bridge использует систему API ключей для аутентификации и авторизации запросов. Система поддерживает:
- ? Настраиваемые API ключи с описанием и владельцем
- ? Права доступа (ReadOnly / ReadWrite)
- ? Срок действия ключей
- ? Управление ключами только с localhost с мастер-паролем
- ? Отслеживание использования ключей

## Настройка

### 1. Установка мастер-пароля

В `appsettings.json` настройте мастер-пароль для управления API ключами:

```json
{
  "ApiKeys": {
    "MasterPassword": "YOUR_SECURE_MASTER_PASSWORD_HERE"
  }
}
```

?? **ВАЖНО**: Измените пароль по умолчанию в production окружении!

### 2. Переменные окружения (рекомендуется для production)

```bash
export ApiKeys__MasterPassword="your-secure-master-password"
```

или в Docker:

```bash
docker run -e ApiKeys__MasterPassword="your-secure-master-password" cdcbridge-host
```

## Управление API ключами

### Создание нового API ключа

**Endpoint**: `POST /api/admin/apikeys`  
**Ограничение**: Только с localhost  
**Требуется**: Мастер-пароль

```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Production Integration",
    "owner": "integration-team",
    "description": "API key for production webhook integration",
    "permission": 1,
    "expiresInDays": 365,
  "masterPassword": "YOUR_MASTER_PASSWORD"
  }'
```

**Параметры**:
- `name` (обязательно): Уникальное имя ключа
- `owner` (опционально): Владелец ключа
- `description` (опционально): Описание назначения
- `permission`: 
  - `0` = ReadOnly (только GET запросы)
  - `1` = ReadWrite (все методы)
- `expiresInDays` (опционально): Срок действия в днях
- `masterPassword` (обязательно): Мастер-пароль

**Ответ**:
```json
{
  "id": 1,
  "key": "abcd1234efgh5678ijkl9012mnop3456",
  "name": "Production Integration",
  "owner": "integration-team",
  "description": "API key for production webhook integration",
  "permission": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2025-01-15T10:30:00Z",
  "isActive": true
}
```

?? **ВАЖНО**: Сохраните значение `key` - оно показывается только один раз при создании!

### Просмотр всех API ключей

**Endpoint**: `GET /api/admin/apikeys?masterPassword=YOUR_MASTER_PASSWORD`  
**Ограничение**: Только с localhost

```bash
curl http://localhost:8080/api/admin/apikeys?masterPassword=YOUR_MASTER_PASSWORD
```

**Ответ**:
```json
[
  {
 "id": 1,
    "name": "Production Integration",
    "owner": "integration-team",
    "description": "API key for production webhook integration",
    "permission": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "expiresAt": "2025-01-15T10:30:00Z",
    "isActive": true,
    "lastUsedAt": "2024-01-15T15:45:00Z",
    "keyPrefix": "abcd1234..."
  }
]
```

### Деактивация API ключа

**Endpoint**: `PUT /api/admin/apikeys/{id}/deactivate?masterPassword=YOUR_MASTER_PASSWORD`  
**Ограничение**: Только с localhost

```bash
curl -X PUT "http://localhost:8080/api/admin/apikeys/1/deactivate?masterPassword=YOUR_MASTER_PASSWORD"
```

### Активация API ключа

**Endpoint**: `PUT /api/admin/apikeys/{id}/activate?masterPassword=YOUR_MASTER_PASSWORD`  
**Ограничение**: Только с localhost

```bash
curl -X PUT "http://localhost:8080/api/admin/apikeys/1/activate?masterPassword=YOUR_MASTER_PASSWORD"
```

### Удаление API ключа

**Endpoint**: `DELETE /api/admin/apikeys/{id}?masterPassword=YOUR_MASTER_PASSWORD`  
**Ограничение**: Только с localhost

```bash
curl -X DELETE "http://localhost:8080/api/admin/apikeys/1?masterPassword=YOUR_MASTER_PASSWORD"
```

## Использование API ключей

### Для клиентов API

Все запросы к API (кроме `/api/admin`, `/swagger`, `/health`) требуют API ключ в заголовке:

```bash
curl -X GET http://localhost:8080/api/metrics \
  -H "X-API-Key: abcd1234efgh5678ijkl9012mnop3456"
```

### Права доступа

#### ReadOnly (Permission = 0)
- ? Разрешено: GET запросы
- ? Запрещено: POST, PUT, PATCH, DELETE

Пример:
```bash
# Работает
curl -X GET http://localhost:8080/api/metrics \
  -H "X-API-Key: readonly-key-here"

# Вернет 403 Forbidden
curl -X POST http://localhost:8080/api/events \
  -H "X-API-Key: readonly-key-here" \
  -H "Content-Type: application/json" \
  -d '{"data": "value"}'
```

#### ReadWrite (Permission = 1)
- ? Разрешено: Все HTTP методы (GET, POST, PUT, PATCH, DELETE)

### В Swagger UI

1. Откройте Swagger UI: `http://localhost:8080/swagger`
2. Нажмите кнопку **Authorize** в правом верхнем углу
3. Введите ваш API ключ в поле **Value**
4. Нажмите **Authorize**, затем **Close**

Теперь все запросы из Swagger будут автоматически включать ваш API ключ.

## Безопасность

### Рекомендации

1. **Храните мастер-пароль в безопасности**
   - Используйте переменные окружения
   - Не коммитьте в Git
   - Используйте секреты (Azure Key Vault, Kubernetes Secrets, и т.д.)

2. **Регулярно ротируйте API ключи**
   - Устанавливайте срок действия
 - Создавайте новые ключи перед истечением старых
   - Деактивируйте неиспользуемые ключи

3. **Используйте принцип минимальных привилегий**
   - Для мониторинга используйте ReadOnly ключи
   - ReadWrite только для автоматизации

4. **Мониторьте использование**
   - Проверяйте `lastUsedAt` для обнаружения неактивных ключей
   - Анализируйте логи на предмет подозрительной активности

### Ограничения по localhost

Управление API ключами возможно **только с localhost**. Это означает:
- ? С сервера, где запущен CDC Bridge
- ? Через SSH туннель
- ? Удаленно через сеть

Пример использования SSH туннеля:
```bash
ssh -L 8080:localhost:8080 user@server
curl -X POST http://localhost:8080/api/admin/apikeys ...
```

## Примеры использования

### Сценарий 1: Создание ключа для CI/CD

```bash
# Создаем ключ с правами ReadWrite для CI/CD
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "GitHub Actions",
    "owner": "DevOps Team",
    "description": "Automated deployment pipeline",
    "permission": 1,
    "expiresInDays": 90,
    "masterPassword": "master-password"
  }'

# Сохраните возвращенный ключ в GitHub Secrets
```

### Сценарий 2: Создание ключа для мониторинга

```bash
# Создаем ReadOnly ключ для Grafana/Prometheus
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
 "name": "Grafana Monitoring",
    "owner": "Monitoring Team",
    "description": "Read-only access for metrics collection",
    "permission": 0,
    "masterPassword": "master-password"
  }'
```

### Сценарий 3: Ротация ключа

```bash
# 1. Создаем новый ключ
NEW_KEY=$(curl -X POST http://localhost:8080/api/admin/apikeys \
-H "Content-Type: application/json" \
  -d '{
    "name": "Production Integration v2",
    "owner": "integration-team",
    "permission": 1,
    "expiresInDays": 365,
    "masterPassword": "master-password"
  }' | jq -r '.key')

# 2. Обновляем конфигурацию клиентов с новым ключом

# 3. Деактивируем старый ключ
curl -X PUT "http://localhost:8080/api/admin/apikeys/1/deactivate?masterPassword=master-password"
```

## Troubleshooting

### Ошибка: "API Key is missing"

Проверьте, что заголовок `X-API-Key` присутствует в запросе.

### Ошибка: "Invalid or inactive API Key"

- Проверьте правильность ключа
- Убедитесь, что ключ активен
- Проверьте срок действия

### Ошибка: "API Key has expired"

Создайте новый ключ и обновите конфигурацию клиентов.

### Ошибка: "This API Key has read-only permissions"

Используйте ReadWrite ключ для операций изменения данных.

### Ошибка: "Access denied. Only localhost is allowed"

Управление ключами возможно только с localhost. Используйте SSH туннель или выполняйте команды непосредственно на сервере.

## Миграция с JWT

Если вы ранее использовали JWT аутентификацию:

1. Создайте API ключи для всех существующих клиентов
2. Обновите клиентов для использования `X-API-Key` заголовка вместо `Authorization: Bearer`
3. Удалите старую JWT конфигурацию из `appsettings.json`

Пример обновления клиента:

**Было (JWT)**:
```bash
curl -H "Authorization: Bearer eyJhbGc..." http://api/metrics
```

**Стало (API Key)**:
```bash
curl -H "X-API-Key: abcd1234..." http://api/metrics
```
