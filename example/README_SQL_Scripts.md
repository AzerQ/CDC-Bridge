# SQL Scripts для настройки CDC Bridge Demo

Этот каталог содержит SQL скрипты для настройки демонстрационной среды CDC Bridge.

## Описание скриптов

### 1. `01_initialize_schema.sql`
**Назначение**: Инициализация схемы базы данных
- Создает базу данных `CdcBridgeDemo`
- Создает схему `demo` 
- Создает следующие таблицы:
  - `Customers` - клиенты
  - `Orders` - заказы  
  - `Products` - товары
  - `OrderItems` - позиции заказов
  - `UserActions` - действия пользователей (для аудита)
- Создает индексы для оптимизации производительности
- Создает триггеры для автоматического обновления поля `LastUpdated`

### 2. `02_setup_cdc.sql`
**Назначение**: Настройка CDC (Change Data Capture)
- Включает CDC на уровне базы данных
- Настраивает CDC для всех таблиц в схеме `demo`
- Создает роли для доступа к CDC данным
- Настраивает параметры очистки и захвата данных
- Создает вспомогательные представления и функции для работы с CDC

### 3. `03_populate_data.sql`
**Назначение**: Заполнение таблиц тестовыми данными
- Добавляет реалистичные тестовые данные во все таблицы
- Создает взаимосвязанные записи (клиенты → заказы → позиции заказов)
- Генерирует дополнительные изменения для демонстрации CDC
- Выводит статистику созданных данных

## Порядок выполнения

Скрипты должны выполняться в следующем порядке:

```sql
-- 1. Сначала создать схему базы данных
:r 01_initialize_schema.sql

-- 2. Затем настроить CDC
:r 02_setup_cdc.sql

-- 3. Наконец, заполнить данными
:r 03_populate_data.sql
```

## Требования

### Системные требования:
- SQL Server 2012 или выше (рекомендуется SQL Server 2019+)
- SQL Server Agent должен быть запущен (для полной функциональности CDC)
- Права `sysadmin` или `db_owner` для настройки CDC

### Предварительная настройка:
1. Убедитесь, что SQL Server Agent запущен
2. Убедитесь, что у вас есть необходимые права доступа
3. При необходимости измените имя базы данных в скриптах

## Использование с различными версиями SQL Server

### SQL Server Express
- CDC поддерживается, но без SQL Server Agent
- Некоторые функции очистки могут быть ограничены
- Скрипты содержат соответствующие предупреждения

### SQL Server Standard/Enterprise
- Полная поддержка всех функций CDC
- Автоматическая очистка устаревших данных
- Оптимальная производительность

## Структура данных

После выполнения скриптов будет создана следующая структура:

```
CdcBridgeDemo
├── demo (схема)
│   ├── Customers (10+ записей)
│   ├── Orders (25+ записей)  
│   ├── Products (15+ записей)
│   ├── OrderItems (50+ записей)
│   └── UserActions (50+ записей)
└── cdc (схема CDC)
    ├── demo_Customers_CT
    ├── demo_Orders_CT
    ├── demo_Products_CT
    ├── demo_OrderItems_CT
    └── demo_UserActions_CT
```

## Тестирование CDC

После выполнения всех скриптов можно протестировать CDC следующими командами:

```sql
-- Просмотр изменений в таблице Customers
SELECT * FROM cdc.demo_Customers_CT ORDER BY __$start_lsn DESC;

-- Использование встроенных функций CDC
DECLARE @from_lsn binary(10) = sys.fn_cdc_get_min_lsn('demo_Customers');
DECLARE @to_lsn binary(10) = sys.fn_cdc_get_max_lsn();

SELECT * FROM cdc.fn_cdc_get_all_changes_demo_Customers(@from_lsn, @to_lsn, 'all');

-- Просмотр через созданное представление
SELECT * FROM demo.vw_cdc_customers_changes ORDER BY StartLSN DESC;
```

## Создание изменений для тестирования

Для генерации новых CDC событий выполните:

```sql
-- Добавление нового клиента
INSERT INTO demo.Customers (FirstName, LastName, Email, Phone, DateOfBirth, IsActive)
VALUES ('Тест', 'Тестов', 'test@example.com', '+7-900-TEST-TEST', '1990-01-01', 1);

-- Обновление клиента  
UPDATE demo.Customers 
SET Phone = '+7-900-NEW-PHONE' 
WHERE Email = 'test@example.com';

-- Удаление клиента
DELETE FROM demo.Customers WHERE Email = 'test@example.com';

-- Просмотр новых изменений
SELECT * FROM cdc.demo_Customers_CT ORDER BY __$start_lsn DESC;
```

## Очистка и сброс

Для полной очистки созданной среды:

```sql
-- Отключение CDC для всех таблиц
EXEC sys.sp_cdc_disable_table @source_schema = 'demo', @source_name = 'Customers', @capture_instance = 'demo_Customers';
EXEC sys.sp_cdc_disable_table @source_schema = 'demo', @source_name = 'Orders', @capture_instance = 'demo_Orders';
EXEC sys.sp_cdc_disable_table @source_schema = 'demo', @source_name = 'Products', @capture_instance = 'demo_Products';
EXEC sys.sp_cdc_disable_table @source_schema = 'demo', @source_name = 'OrderItems', @capture_instance = 'demo_OrderItems';
EXEC sys.sp_cdc_disable_table @source_schema = 'demo', @source_name = 'UserActions', @capture_instance = 'demo_UserActions';

-- Отключение CDC на уровне базы данных
EXEC sys.sp_cdc_disable_db;

-- Удаление базы данных
DROP DATABASE CdcBridgeDemo;
```

## Troubleshooting

### Частые проблемы:

1. **Ошибка прав доступа**
   - Убедитесь, что у вас есть права `sysadmin` или `db_owner`

2. **SQL Server Agent не запущен**
   - Запустите службу SQL Server Agent через SQL Server Configuration Manager

3. **CDC не отслеживает изменения**
   - Проверьте, что CDC включен: `SELECT is_cdc_enabled FROM sys.databases WHERE name = 'CdcBridgeDemo'`
   - Проверьте CDC jobs: `SELECT * FROM msdb.dbo.cdc_jobs`

4. **Проблемы с производительностью**
   - Настройте параметры retention и polling interval в скрипте `02_setup_cdc.sql`

### Полезные запросы для диагностики:

```sql
-- Проверка статуса CDC на уровне БД
SELECT name, is_cdc_enabled FROM sys.databases WHERE name = 'CdcBridgeDemo';

-- Список таблиц с включенным CDC
SELECT * FROM cdc.change_tables;

-- Проверка CDC jobs
SELECT * FROM msdb.dbo.cdc_jobs WHERE database_id = DB_ID('CdcBridgeDemo');

-- Диапазон LSN для таблицы
SELECT 
    sys.fn_cdc_get_min_lsn('demo_Customers') AS min_lsn,
    sys.fn_cdc_get_max_lsn() AS max_lsn;
```

## Интеграция с CDC Bridge

Эти скрипты создают готовую среду для тестирования CDC Bridge. После выполнения скриптов:

1. Настройте connection string для подключения к `CdcBridgeDemo`
2. Укажите таблицы из схемы `demo` в конфигурации CDC Bridge
3. Запустите CDC Bridge для мониторинга изменений
4. Выполняйте операции INSERT/UPDATE/DELETE для генерации событий CDC

Созданная структура полностью совместима с архитектурой CDC Bridge и предоставляет реалистичные данные для тестирования.