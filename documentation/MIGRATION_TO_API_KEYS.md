# Миграция на API Key Authentication

## Изменения в системе аутентификации

CDC Bridge перешел с JWT-based аутентификации на более гибкую систему API ключей.

### Что изменилось

#### Было (JWT)
- ? Регистрация пользователей через `/api/auth/register`
- ? Вход через `/api/auth/login` с получением JWT токена
- ? Токены с ограниченным сроком действия
- ? Необходимость обновления токенов
- ? Сложное управление доступом

#### Стало (API Keys)
- ? Создание API ключей через `/api/admin/apikeys` (только localhost)
- ? Настраиваемые права доступа (ReadOnly/ReadWrite)
- ? Настраиваемый срок действия
- ? Описание и владелец ключа
- ? Отслеживание использования
- ? Защита мастер-паролем
- ? Управление только с localhost

### Преимущества новой системы

1. **Безопасность**
   - Управление ключами только с сервера (localhost)
   - Мастер-пароль для всех операций
   - Granular permissions (ReadOnly/ReadWrite)

2. **Простота использования**
   - Один ключ вместо JWT токена
   - Не нужно обновлять токены
   - Просто добавить заголовок `X-API-Key`

3. **Управление**
   - Легко деактивировать скомпрометированные ключи
   - Отслеживание последнего использования
   - Описательные имена и владельцы

4. **Аудит**
- История использования каждого ключа
   - Возможность отследить, какой ключ используется

## Инструкция по миграции

### Шаг 1: Обновите конфигурацию

Удалите или закомментируйте секцию JWT в `appsettings.json`:

```json
{
  "ApiKeys": {
    "MasterPassword": "YOUR_SECURE_MASTER_PASSWORD"
  }
}
```

### Шаг 2: Создайте API ключи

Для каждого существующего пользователя или интеграции создайте API ключ:

```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Integration Name",
    "owner": "Team/User Name",
    "description": "Purpose of this key",
    "permission": 1,
"expiresInDays": 365,
    "masterPassword": "YOUR_MASTER_PASSWORD"
  }'
```

**Сохраните возвращенный ключ!** Он показывается только один раз.

### Шаг 3: Обновите клиентов

#### Было (JWT):
```bash
# 1. Получение токена
TOKEN=$(curl -X POST http://api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass"}' \
  | jq -r '.token')

# 2. Использование токена
curl -H "Authorization: Bearer $TOKEN" http://api/metrics
```

#### Стало (API Key):
```bash
# Просто используйте API ключ
curl -H "X-API-Key: your-api-key-here" http://api/metrics
```

### Шаг 4: Обновите документацию

Обновите внутреннюю документацию и инструкции для разработчиков:
- Удалите ссылки на `/api/auth/register` и `/api/auth/login`
- Добавьте инструкции по получению API ключей
- Обновите примеры кода

### Шаг 5: Проверка

Убедитесь, что все интеграции работают с новыми API ключами:

```bash
# Тест ReadOnly ключа
curl -H "X-API-Key: readonly-key" http://api/metrics

# Тест ReadWrite ключа
curl -X POST -H "X-API-Key: readwrite-key" \
  -H "Content-Type: application/json" \
  -d '{"data":"value"}' \
  http://api/events
```

## Удаленные компоненты

Следующие файлы и компоненты были удалены:
- `src/CdcBridge.Core/Models/User.cs` (если существовал)
- `src/CdcBridge.Host/Api/Services/JwtService.cs` (если существовал)
- `src/CdcBridge.Host/Api/Services/PasswordHasher.cs` (если существовал)
- `src/CdcBridge.Host/Api/Controllers/AuthController.cs` (если существовал)
- JWT конфигурация в `Program.cs`

## Новые компоненты

Добавлены следующие файлы:
- `src/CdcBridge.Core/Models/ApiKey.cs` - Модель API ключа
- `src/CdcBridge.Host/Middleware/ApiKeyAuthenticationMiddleware.cs` - Middleware для проверки ключей
- `src/CdcBridge.Host/Api/Controllers/AdminController.cs` - Управление ключами
- `API_KEY_AUTHENTICATION.md` - Документация по API ключам
- `src/CdcBridge.Host/ApiKeyManagement.http` - Примеры запросов

## База данных

Добавлена новая таблица `ApiKeys` со следующей структурой:
- `Id` - Уникальный идентификатор
- `Key` - API ключ (уникальный)
- `Name` - Имя ключа
- `Owner` - Владелец
- `Description` - Описание
- `Permission` - Права доступа (0=ReadOnly, 1=ReadWrite)
- `CreatedAt` - Дата создания
- `ExpiresAt` - Дата истечения
- `IsActive` - Активен ли ключ
- `LastUsedAt` - Время последнего использования

Миграция применяется автоматически при запуске приложения.

## Вопросы и ответы

### Q: Как получить первый API ключ?
A: Только с localhost, используя мастер-пароль из конфигурации:
```bash
curl -X POST http://localhost:8080/api/admin/apikeys \
  -H "Content-Type: application/json" \
  -d '{"name":"First Key","permission":1,"masterPassword":"YOUR_MASTER_PASSWORD"}'
```

### Q: Можно ли создать ключ удаленно?
A: Нет, управление ключами возможно только с localhost. Используйте SSH туннель:
```bash
ssh -L 8080:localhost:8080 user@server
```

### Q: Что делать, если ключ скомпрометирован?
A: Немедленно деактивируйте его:
```bash
curl -X PUT "http://localhost:8080/api/admin/apikeys/{id}/deactivate?masterPassword=MASTER_PASSWORD"
```

### Q: Как ротировать ключи?
A: 
1. Создайте новый ключ
2. Обновите конфигурацию клиентов
3. Деактивируйте старый ключ
4. Через некоторое время удалите старый ключ

### Q: Можно ли восстановить JWT аутентификацию?
A: Технически да, но не рекомендуется. API ключи обеспечивают лучшую безопасность и управляемость для API интеграций.

## Поддержка

Для вопросов и проблем:
- Создайте issue в репозитории
- Обратитесь к документации: `API_KEY_AUTHENTICATION.md`
- Проверьте примеры: `src/CdcBridge.Host/ApiKeyManagement.http`
