# CDC-Bridge Example Application

Это пример приложения для демонстрации работы с Change Data Capture (CDC) в SQL Server. Приложение состоит из двух частей:

1. **CdcGenerator** - консольное приложение для создания таблиц, включения CDC и генерации данных
2. **CdcWebhookReceiver** - веб-приложение ASP.NET для приема и отображения изменений

> **Примечание**: Отправку CDC-событий из базы данных в веб-приложение выполняет Core сервис через плагины (WebhookSinkPlugin), а не консольное приложение.

## Требования

- .NET 7.0 SDK или выше
- SQL Server (локальный или удаленный)
- Права администратора в SQL Server (для включения CDC)

## Структура проекта

```
ExampleApp/
├── CdcGenerator/              # Консольное приложение
│   ├── CdcGenerator.csproj    # Файл проекта
│   └── Program.cs             # Основной код приложения
│
└── CdcWebhookReceiver/        # Веб-приложение
    ├── CdcWebhookReceiver.csproj  # Файл проекта
    ├── Program.cs             # Точка входа приложения
    ├── Controllers/           # Контроллеры API
    │   ├── WebhookController.cs  # Контроллер для приема webhook-уведомлений
    │   └── EventsController.cs   # Контроллер для отображения данных
    ├── Models/                # Модели данных
    │   └── CdcEvent.cs        # Модель события CDC
    └── Services/              # Сервисы
        └── ChangeDataCaptureService.cs  # Сервис для хранения и управления данными
```

## Настройка и запуск

### 1. Настройка строки подключения

В файле `CdcGenerator/Program.cs` настройте строку подключения к вашему SQL Server:

```csharp
private static readonly string ConnectionString = "Server=.;Database=CdcExampleDb;Integrated Security=True;TrustServerCertificate=True";
```

### 2. Запуск веб-приложения

```bash
cd CdcWebhookReceiver
dotnet run
```

Веб-приложение будет запущено по адресу http://localhost:5000

### 3. Запуск консольного приложения

```bash
cd CdcGenerator
dotnet run
```

Консольное приложение выполнит следующие действия:
1. Создаст базу данных и таблицу, если они не существуют
2. Включит CDC для базы данных и таблицы
3. Сгенерирует и вставит фейковые данные с помощью Bogus
4. Выполнит несколько операций обновления и удаления

## API веб-приложения

### Прием webhook-уведомлений

- **URL**: `/api/webhook`
- **Метод**: POST
- **Тело запроса**: JSON с массивом CDC-событий

### Получение всех CDC-событий

- **URL**: `/api/events`
- **Метод**: GET
- **Ответ**: JSON с массивом всех CDC-событий

### Получение CDC-событий по таблице

- **URL**: `/api/events/table/{tableName}`
- **Метод**: GET
- **Ответ**: JSON с массивом CDC-событий для указанной таблицы

### Получение CDC-событий по типу операции

- **URL**: `/api/events/operation/{operation}`
- **Метод**: GET
- **Ответ**: JSON с массивом CDC-событий для указанного типа операции (insert, update, delete)

### Очистка всех CDC-событий

- **URL**: `/api/webhook`
- **Метод**: DELETE
- **Ответ**: JSON с сообщением об успешной очистке

## Примечания

- Для работы CDC требуется SQL Server Enterprise, Developer или Standard Edition
- В реальном приложении необходимо настроить безопасность и аутентификацию для webhook-эндпоинта
- Для продакшн-среды рекомендуется использовать более надежное хранилище данных вместо хранения в памяти