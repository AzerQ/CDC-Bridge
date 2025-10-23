# ?? ВАЖНОЕ ОБНОВЛЕНИЕ: Переход на CdcBridge.Host

## ?? Уведомление для команды разработки

**Дата:** 20 октября 2025  
**Приоритет:** ВЫСОКИЙ  
**Действие требуется:** Обновить локальные окружения

---

## ?? Что изменилось?

Проекты `CdcBridge.Api` и `CdcBridge.Worker` **объединены** в один проект `CdcBridge.Host`.

### Причины объединения:

? Устранение дублирования кода  
? Упрощение развёртывания (1 контейнер вместо 2)  
? Единая точка конфигурации  
? Упрощённая поддержка и разработка  

---

## ?? Что нужно сделать разработчикам?

### 1. Обновите код из репозитория

```bash
git pull origin main
```

### 2. Пересоберите решение

```bash
dotnet clean
dotnet build CDC-Bridge.sln
```

### 3. Обновите Docker окружения (если используете)

**Старая команда:**
```bash
docker-compose up -d
```

**Новая команда:**
```bash
docker-compose -f docker-compose.host.yml up -d
```

### 4. Обновите launch settings в IDE

**Для Visual Studio / Rider:**
- Удалите launch configurations для `CdcBridge.Api` и `CdcBridge.Worker`
- Добавьте launch configuration для `CdcBridge.Host`

**Для VS Code (launch.json):**
```json
{
    "name": "CdcBridge.Host",
    "type": "coreclr",
    "request": "launch",
    "preLaunchTask": "build",
    "program": "${workspaceFolder}/src/CdcBridge.Host/bin/Debug/net8.0/CdcBridge.Host.dll",
    "args": [],
    "cwd": "${workspaceFolder}/src/CdcBridge.Host",
    "stopAtEntry": false,
    "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
    }
}
```

---

## ?? Что осталось прежним?

### Функциональность

- ? Все API endpoints работают как раньше
- ? Background Services работают как раньше
- ? Конфигурация `cdc-settings.yaml` не изменилась
- ? База данных и миграции не изменились

### API Endpoints (без изменений)

```
GET  /api/metrics
GET  /api/events
GET  /api/events/{id}
GET  /api/logs
GET  /api/configuration
GET  /api/configuration/tracking-instances
GET  /api/configuration/receivers
```

### Background Services (без изменений)

- ReceiverWorker
- CleanupWorker

---

## ?? Локальная разработка

### Запуск приложения

**Через dotnet CLI:**
```bash
cd src/CdcBridge.Host
dotnet run
```

**Через Docker:**
```bash
docker build -t cdcbridge-host -f src/CdcBridge.Host/Dockerfile .
docker run -p 8080:8080 cdcbridge-host
```

### Доступ к приложению

- **API:** http://localhost:8080
- **Swagger UI:** http://localhost:8080/swagger (только в Development)

---

## ?? Тестирование

### Запуск всех тестов

```bash
dotnet test
```

### Тестирование API

Используйте файл `src/CdcBridge.Host/CdcBridge.Host.http` для тестирования endpoints через HTTP Client в VS Code или Rider.

---

## ?? Известные проблемы

### Предупреждение о SQLite RID

При сборке вы можете увидеть предупреждение:
```
warning NETSDK1206: обнаружены идентификаторы среды выполнения, зависящие от версии или дистрибутива: win7-x64, win7-x86
```

**Это НЕ критично** - это связано с SQLite пакетом и не влияет на работу приложения.

---

## ?? Дополнительная документация

- [Руководство по миграции](MIGRATION_TO_HOST.md)
- [Сводка изменений](CHANGES_SUMMARY.md)
- [Архивные проекты](ARCHIVED_PROJECTS.md)
- [Новый README](README.md)

---

## ? FAQ

### Вопрос: Куда делись мои настройки из appsettings.json?

**Ответ:** Все настройки объединены в `src/CdcBridge.Host/appsettings.json`. Проверьте новый файл - там должно быть всё.

### Вопрос: Нужно ли обновлять cdc-settings.yaml?

**Ответ:** Нет, формат файла не изменился. Используйте существующий файл.

### Вопрос: Работает ли API так же, как раньше?

**Ответ:** Да, все endpoints работают идентично. Никаких breaking changes в API.

### Вопрос: Что делать, если что-то сломалось?

**Ответ:** 
1. Проверьте, что вы сделали `dotnet clean` и `dotnet build`
2. Проверьте, что используете правильный проект для запуска (`CdcBridge.Host`)
3. Если проблема не решается - свяжитесь с командой

### Вопрос: Можно ли вернуться к старым проектам?

**Ответ:** Технически да, через git revert, но это не рекомендуется. Все функции доступны в новом проекте.

---

## ?? Контакты для вопросов

Если у вас возникли вопросы или проблемы:

1. Создайте issue в репозитории
2. Напишите в канал команды
3. Обратитесь к архитектору проекта

---

## ? Чеклист для разработчика

Отметьте, когда выполните:

- [ ] Выполнил `git pull`
- [ ] Выполнил `dotnet clean && dotnet build`
- [ ] Обновил launch settings в IDE
- [ ] Протестировал локальный запуск
- [ ] Обновил Docker окружение (если использую)
- [ ] Ознакомился с новой документацией
- [ ] Готов к работе с новой структурой проекта

---

**Спасибо за понимание и сотрудничество! ??**
