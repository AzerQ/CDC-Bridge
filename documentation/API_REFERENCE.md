# Справочник REST API CDC Bridge

API предоставляет возможности для мониторинга состояния системы, просмотра событий, логов и управления конфигурацией. Все запросы (кроме создания первого ключа) требуют наличия заголовка `X-API-Key`.

---

## 1. Аутентификация

Система использует API ключи.
*   **Заголовок:** `X-API-Key: your-api-key-here`
*   **Управление ключами:** Осуществляется через эндпоинты `/api/Admin` (доступно только с localhost).

---

## 2. Метрики и состояние

### Получение общих метрик
`GET /api/Metrics`
Возвращает агрегированную информацию о работе системы.
*   **Ответ:**
    ```json
    {
      "totalEventsProcessed": 1500,
      "activeTrackingInstances": 5,
      "activeReceivers": 3,
      "failedDeliveries": 12,
      "uptimeSeconds": 36000
    }
    ```

---

## 3. События (Events)

### Список событий
`GET /api/Events`
Возвращает список захваченных событий с поддержкой фильтрации и пагинации.
*   **Параметры:** `page`, `pageSize`, `trackingInstance`, `status`.

### Детали события
`GET /api/Events/{id}`
Возвращает полную информацию о конкретном событии, включая данные изменений и статусы доставки по всем приемникам.

---

## 4. Конфигурация

### Текущая конфигурация
`GET /api/Configuration`
Возвращает объединенную конфигурацию из YAML файла.

### Список источников
`GET /api/Configuration/tracking-instances`

### Список приемников
`GET /api/Configuration/receivers`

---

## 5. Логирование

### Просмотр логов
`GET /api/Logs`
Позволяет просматривать системные логи, хранящиеся в SQLite.
*   **Параметры:** `level`, `messageSearch`, `page`, `pageSize`.

---

## 6. Администрирование (Admin)
*Доступно только с localhost*

### Создание API ключа
`POST /api/Admin/apikeys`
*   **Тело запроса:**
    ```json
    {
      "name": "My App",
      "owner": "Dev Team",
      "permission": 1,
      "expiresInDays": 365,
      "masterPassword": "YOUR_MASTER_PASSWORD"
    }
    ```

### Управление ключами
*   `GET /api/Admin/apikeys?masterPassword=...` — список всех ключей.
*   `PUT /api/Admin/apikeys/{id}/deactivate` — деактивация ключа.
*   `DELETE /api/Admin/apikeys/{id}` — удаление ключа.
