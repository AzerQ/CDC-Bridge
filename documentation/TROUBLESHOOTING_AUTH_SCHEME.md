# Решение проблемы: System.InvalidOperationException - No authenticationScheme was specified

## Проблема

При запросах к API возникала ошибка:
```
System.InvalidOperationException: No authenticationScheme was specified, and there was no DefaultChallengeScheme found.
```

## Причина

После миграции с JWT аутентификации на API ключи, в контроллерах остались атрибуты `[Authorize]`, которые требуют настроенной схемы аутентификации ASP.NET Core (например, JWT Bearer). Однако в новой системе аутентификация обрабатывается через custom middleware `ApiKeyAuthenticationMiddleware`, который не использует стандартные схемы ASP.NET Core.

## Решение

Удалены атрибуты `[Authorize]` из всех контроллеров, так как аутентификация теперь обрабатывается на уровне middleware, который выполняется перед маршрутизацией запросов к контроллерам.

### Изменения в файлах

1. **`src/CdcBridge.Host/Api/Controllers/MetricsController.cs`**
   - Удален `using Microsoft.AspNetCore.Authorization;`
   - Удален атрибут `[Authorize]` с контроллера

2. **`src/CdcBridge.Host/Api/Controllers/EventsController.cs`**
- Удален `using Microsoft.AspNetCore.Authorization;`
   - Удален атрибут `[Authorize]` с контроллера

3. **`src/CdcBridge.Host/Api/Controllers/LogsController.cs`**
   - Удален `using Microsoft.AspNetCore.Authorization;`
   - Удален атрибут `[Authorize]` с контроллера

4. **`src/CdcBridge.Host/Api/Controllers/ConfigurationController.cs`**
   - Удален `using Microsoft.AspNetCore.Authorization;`
   - Удален атрибут `[Authorize]` с контроллера

### Почему это работает

**ApiKeyAuthenticationMiddleware** настроен в `Program.cs` следующим образом:

```csharp
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
```

Этот middleware выполняется **перед** маршрутизацией к контроллерам и:
1. Проверяет наличие заголовка `X-API-Key`
2. Валидирует ключ в базе данных
3. Проверяет срок действия
4. Проверяет права доступа (ReadOnly/ReadWrite)
5. Возвращает 401/403 если проверки не прошли
6. Передает управление контроллеру если все проверки успешны

**AdminController** не затронут, так как он использует собственную логику проверки localhost и мастер-пароля внутри методов, а не через атрибуты авторизации.

## Проверка

После внесения изменений:
1. Выполнена очистка проекта: `dotnet clean CDC-Bridge.sln`
2. Выполнена сборка: `dotnet build CDC-Bridge.sln`
3. Сборка успешна без ошибок

## Тестирование

### Тест 1: Запрос без API ключа (должен вернуть 401)
```bash
curl http://localhost:8080/api/metrics
```
**Ожидаемый результат:**
```json
{"error": "API Key is missing. Please provide X-API-Key header."}
```

### Тест 2: Запрос с валидным API ключом (должен вернуть 200)
```bash
curl -H "X-API-Key: your-valid-key" http://localhost:8080/api/metrics
```
**Ожидаемый результат:** Успешный ответ с метриками

### Тест 3: Запрос с ReadOnly ключом на POST endpoint (должен вернуть 403)
```bash
curl -X POST -H "X-API-Key: readonly-key" \
  -H "Content-Type: application/json" \
  -d '{"data":"value"}' \
  http://localhost:8080/api/events
```
**Ожидаемый результат:**
```json
{"error": "This API Key has read-only permissions"}
```

## Дополнительная информация

- Документация по API ключам: `API_KEY_AUTHENTICATION.md`
- Руководство по миграции: `MIGRATION_TO_API_KEYS.md`
- Примеры HTTP запросов: `src/CdcBridge.Host/ApiKeyManagement.http`
