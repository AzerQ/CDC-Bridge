-- =======================================================
-- Скрипт для настройки CDC (Change Data Capture)
-- Включение CDC на уровне базы данных и настройка для таблиц
-- =======================================================

-- Убеждаемся, что мы подключены к правильной базе данных
USE [CdcBridgeDemo]
GO

-- =======================================================
-- ПРОВЕРКА ПРЕДВАРИТЕЛЬНЫХ УСЛОВИЙ
-- =======================================================

-- Проверяем, что SQL Server Agent запущен (необходимо для CDC)
IF (SELECT @@SERVICENAME) NOT LIKE '%SQLEXPRESS%'
BEGIN
    DECLARE @AgentStatus NVARCHAR(50)
    EXEC xp_servicecontrol 'QueryState', N'SQLServerAGENT'
    
    -- Примечание: В production среде SQL Server Agent должен быть запущен
    PRINT 'Внимание: Убедитесь, что SQL Server Agent запущен для корректной работы CDC'
END
ELSE
BEGIN
    PRINT 'Предупреждение: SQL Server Express не поддерживает SQL Server Agent'
    PRINT 'CDC может работать, но клининг и мониторинг могут быть ограничены'
END

-- Проверяем права доступа
IF IS_SRVROLEMEMBER('sysadmin') = 1 OR IS_MEMBER('db_owner') = 1
BEGIN
    PRINT 'Права доступа: OK'
END
ELSE
BEGIN
    PRINT 'ОШИБКА: Недостаточно прав для настройки CDC. Требуются права sysadmin или db_owner.'
    RETURN
END

-- =======================================================
-- ВКЛЮЧЕНИЕ CDC НА УРОВНЕ БАЗЫ ДАННЫХ
-- =======================================================

-- Проверяем, включен ли CDC на уровне базы данных
IF (SELECT is_cdc_enabled FROM sys.databases WHERE name = 'CdcBridgeDemo') = 0
BEGIN
    PRINT 'Включение CDC на уровне базы данных...'
    
    -- Включаем CDC для базы данных
    EXEC sys.sp_cdc_enable_db
    
    PRINT 'CDC успешно включен на уровне базы данных'
END
ELSE
BEGIN
    PRINT 'CDC уже включен на уровне базы данных'
END

-- Проверяем статус CDC
SELECT 
    name AS DatabaseName,
    is_cdc_enabled AS CDC_Enabled,
    create_date AS DatabaseCreated
FROM sys.databases 
WHERE name = 'CdcBridgeDemo'

-- =======================================================
-- НАСТРОЙКА CDC ДЛЯ ТАБЛИЦЫ CUSTOMERS
-- =======================================================

-- Проверяем, настроен ли CDC для таблицы Customers
IF NOT EXISTS (SELECT * FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('demo.Customers'))
BEGIN
    PRINT 'Настройка CDC для таблицы demo.Customers...'
    
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'demo',
        @source_name = N'Customers',
        @role_name = N'cdc_admin',
        @filegroup_name = N'PRIMARY',
        @supports_net_changes = 1

    PRINT 'CDC настроен для таблицы demo.Customers'
END
ELSE
BEGIN
    PRINT 'CDC уже настроен для таблицы demo.Customers'
END

-- =======================================================
-- НАСТРОЙКА CDC ДЛЯ ТАБЛИЦЫ ORDERS
-- =======================================================

IF NOT EXISTS (SELECT * FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('demo.Orders'))
BEGIN
    PRINT 'Настройка CDC для таблицы demo.Orders...'
    
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'demo',
        @source_name = N'Orders',
        @role_name = N'cdc_admin',
        @filegroup_name = N'PRIMARY',
        @supports_net_changes = 1

    PRINT 'CDC настроен для таблицы demo.Orders'
END
ELSE
BEGIN
    PRINT 'CDC уже настроен для таблицы demo.Orders'
END

-- =======================================================
-- НАСТРОЙКА CDC ДЛЯ ТАБЛИЦЫ PRODUCTS
-- =======================================================

IF NOT EXISTS (SELECT * FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('demo.Products'))
BEGIN
    PRINT 'Настройка CDC для таблицы demo.Products...'
    
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'demo',
        @source_name = N'Products',
        @role_name = N'cdc_admin',
        @filegroup_name = N'PRIMARY',
        @supports_net_changes = 1

    PRINT 'CDC настроен для таблицы demo.Products'
END
ELSE
BEGIN
    PRINT 'CDC уже настроен для таблицы demo.Products'
END

-- =======================================================
-- НАСТРОЙКА CDC ДЛЯ ТАБЛИЦЫ ORDERITEMS
-- =======================================================

IF NOT EXISTS (SELECT * FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('demo.OrderItems'))
BEGIN
    PRINT 'Настройка CDC для таблицы demo.OrderItems...'
    
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'demo',
        @source_name = N'OrderItems',
        @role_name = N'cdc_admin',
        @filegroup_name = N'PRIMARY',
        @supports_net_changes = 1

    PRINT 'CDC настроен для таблицы demo.OrderItems'
END
ELSE
BEGIN
    PRINT 'CDC уже настроен для таблицы demo.OrderItems'
END

-- =======================================================
-- НАСТРОЙКА CDC ДЛЯ ТАБЛИЦЫ USERACTIONS
-- =======================================================

IF NOT EXISTS (SELECT * FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('demo.UserActions'))
BEGIN
    PRINT 'Настройка CDC для таблицы demo.UserActions...'
    
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'demo',
        @source_name = N'UserActions',
        @role_name = N'cdc_admin',
        @filegroup_name = N'PRIMARY',
        @supports_net_changes = 1

    PRINT 'CDC настроен для таблицы demo.UserActions'
END
ELSE
BEGIN
    PRINT 'CDC уже настроен для таблицы demo.UserActions'
END

-- =======================================================
-- СОЗДАНИЕ РОЛИ ДЛЯ ДОСТУПА К CDC
-- =======================================================

-- Создаем роль для доступа к CDC данным
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'cdc_reader' AND type = 'R')
BEGIN
    CREATE ROLE [cdc_reader]
    PRINT 'Создана роль cdc_reader'
END

-- Даем права на чтение CDC данных
GRANT SELECT ON SCHEMA::cdc TO [cdc_reader]

-- =======================================================
-- НАСТРОЙКА ПАРАМЕТРОВ ОЧИСТКИ CDC
-- =======================================================

-- Настраиваем период хранения данных CDC (3 дня = 4320 минут)
EXEC sys.sp_cdc_change_job 
    @job_type = N'cleanup',
    @retention = 4320  -- 3 дня в минутах

-- Настраиваем интервал сканирования для захвата изменений (5 минут)
EXEC sys.sp_cdc_change_job 
    @job_type = N'capture',
    @pollinginterval = 300  -- 5 минут в секундах

-- =======================================================
-- ПРОВЕРКА НАСТРОЕННЫХ CDC ТАБЛИЦ
-- =======================================================

PRINT ''
PRINT '=======================================================';
PRINT 'СВОДКА НАСТРОЕННЫХ CDC ТАБЛИЦ'
PRINT '======================================================='

SELECT 
    ct.source_schema,
    ct.source_table,
    ct.capture_instance,
    ct.object_id,
    ct.source_object_id,
    ct.start_lsn,
    ct.create_date,
    CASE 
        WHEN ct.supports_net_changes = 1 THEN 'Да'
        ELSE 'Нет'
    END AS supports_net_changes
FROM cdc.change_tables ct
ORDER BY ct.source_schema, ct.source_table

-- =======================================================
-- ПРОВЕРКА CDC JOBS
-- =======================================================

PRINT ''
PRINT 'CDC JOBS:'

SELECT 
    job_type,
    database_id,
    continuous,
    pollinginterval,
    retention,
    threshold
FROM msdb.dbo.cdc_jobs 
WHERE database_id = DB_ID('CdcBridgeDemo')

-- =======================================================
-- СОЗДАНИЕ ПРЕДСТАВЛЕНИЙ ДЛЯ УПРОЩЕННОГО ДОСТУПА К CDC
-- =======================================================

-- Представление для изменений в таблице Customers
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_cdc_customers_changes')
BEGIN
    EXEC('
    CREATE VIEW [demo].[vw_cdc_customers_changes]
    AS
    SELECT 
        __$operation AS OperationType,
        __$seqval AS SequenceValue,
        __$start_lsn AS StartLSN,
        __$update_mask AS UpdateMask,
        CustomerId,
        FirstName,
        LastName,
        Email,
        Phone,
        DateOfBirth,
        RegistrationDate,
        IsActive,
        LastUpdated,
        CASE __$operation
            WHEN 1 THEN ''DELETE''
            WHEN 2 THEN ''INSERT''
            WHEN 3 THEN ''UPDATE (Before)''
            WHEN 4 THEN ''UPDATE (After)''
            ELSE ''UNKNOWN''
        END AS OperationDescription
    FROM cdc.demo_Customers_CT
    ')
    PRINT 'Создано представление vw_cdc_customers_changes'
END

-- Предоставляем права на представление
GRANT SELECT ON [demo].[vw_cdc_customers_changes] TO [cdc_reader]

-- =======================================================
-- ПОЛЕЗНЫЕ ФУНКЦИИ ДЛЯ РАБОТЫ С CDC
-- =======================================================

-- Функция для получения диапазона LSN
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'fn_get_cdc_lsn_range' AND type = 'FN')
BEGIN
    EXEC('
    CREATE FUNCTION [demo].[fn_get_cdc_lsn_range](@table_name NVARCHAR(100))
    RETURNS TABLE
    AS
    RETURN
    (
        SELECT 
            @table_name AS TableName,
            sys.fn_cdc_get_min_lsn(@table_name) AS MinLSN,
            sys.fn_cdc_get_max_lsn() AS MaxLSN
    )
    ')
    PRINT 'Создана функция fn_get_cdc_lsn_range'
END

PRINT ''
PRINT '=======================================================';
PRINT 'CDC НАСТРОЕН УСПЕШНО!'
PRINT '======================================================='
PRINT 'Все таблицы demo схемы настроены для отслеживания изменений'
PRINT 'Создана роль cdc_reader для доступа к CDC данным'
PRINT 'Настроены параметры очистки и захвата данных'
PRINT ''
PRINT 'Для мониторинга изменений используйте:'
PRINT '- Таблицы cdc.demo_[TableName]_CT'
PRINT '- Представление demo.vw_cdc_customers_changes'
PRINT '- Функции cdc.fn_cdc_get_all_changes_* и cdc.fn_cdc_get_net_changes_*'

GO