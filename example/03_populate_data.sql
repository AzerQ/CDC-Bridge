-- =======================================================
-- Скрипт для заполнения таблиц тестовыми данными
-- Создание реалистичных данных для демонстрации CDC Bridge
-- =======================================================

USE [CdcBridgeDemo]
GO

-- =======================================================
-- ОЧИСТКА СУЩЕСТВУЮЩИХ ДАННЫХ (опционально)
-- =======================================================

PRINT 'Начинаем заполнение тестовыми данными...'

-- Отключаем проверку внешних ключей для быстрой очистки
-- ALTER TABLE [demo].[OrderItems] NOCHECK CONSTRAINT ALL
-- ALTER TABLE [demo].[Orders] NOCHECK CONSTRAINT ALL

-- TRUNCATE TABLE [demo].[OrderItems]
-- TRUNCATE TABLE [demo].[Orders]
-- TRUNCATE TABLE [demo].[Products] 
-- TRUNCATE TABLE [demo].[Customers]
-- TRUNCATE TABLE [demo].[UserActions]

-- Включаем проверку внешних ключей обратно
-- ALTER TABLE [demo].[Orders] CHECK CONSTRAINT ALL
-- ALTER TABLE [demo].[OrderItems] CHECK CONSTRAINT ALL

-- =======================================================
-- ЗАПОЛНЕНИЕ ТАБЛИЦЫ CUSTOMERS
-- =======================================================

PRINT 'Заполнение таблицы Customers...'

INSERT INTO [demo].[Customers] ([FirstName], [LastName], [Email], [Phone], [DateOfBirth], [RegistrationDate], [IsActive])
VALUES 
    ('Алексей', 'Иванов', 'alexey.ivanov@email.com', '+7-900-123-4567', '1985-03-15', DATEADD(day, -365, GETUTCDATE()), 1),
    ('Мария', 'Петрова', 'maria.petrova@email.com', '+7-900-234-5678', '1990-07-22', DATEADD(day, -300, GETUTCDATE()), 1),
    ('Дмитрий', 'Сидоров', 'dmitry.sidorov@email.com', '+7-900-345-6789', '1988-11-08', DATEADD(day, -250, GETUTCDATE()), 1),
    ('Елена', 'Козлова', 'elena.kozlova@email.com', '+7-900-456-7890', '1992-01-30', DATEADD(day, -200, GETUTCDATE()), 1),
    ('Андрей', 'Новиков', 'andrey.novikov@email.com', '+7-900-567-8901', '1987-09-12', DATEADD(day, -180, GETUTCDATE()), 1),
    ('Ольга', 'Морозова', 'olga.morozova@email.com', '+7-900-678-9012', '1993-05-18', DATEADD(day, -150, GETUTCDATE()), 1),
    ('Сергей', 'Волков', 'sergey.volkov@email.com', '+7-900-789-0123', '1986-12-03', DATEADD(day, -120, GETUTCDATE()), 1),
    ('Анна', 'Лебедева', 'anna.lebedeva@email.com', '+7-900-890-1234', '1991-08-25', DATEADD(day, -90, GETUTCDATE()), 1),
    ('Михаил', 'Соколов', 'mikhail.sokolov@email.com', '+7-900-901-2345', '1989-04-14', DATEADD(day, -60, GETUTCDATE()), 1),
    ('Татьяна', 'Орлова', 'tatyana.orlova@email.com', '+7-900-012-3456', '1994-10-07', DATEADD(day, -30, GETUTCDATE()), 1)

PRINT CONCAT('Добавлено клиентов: ', @@ROWCOUNT)

-- =======================================================
-- ЗАПОЛНЕНИЕ ТАБЛИЦЫ PRODUCTS
-- =======================================================

PRINT 'Заполнение таблицы Products...'

INSERT INTO [demo].[Products] ([ProductName], [SKU], [Category], [Price], [StockQuantity], [IsActive], [CreatedDate])
VALUES 
    ('Смартфон Samsung Galaxy S24', 'PHONE-SAM-S24', 'Электроника', 79999.99, 50, 1, DATEADD(day, -400, GETUTCDATE())),
    ('Ноутбук MacBook Air M2', 'LAPTOP-APL-MBA-M2', 'Компьютеры', 129999.99, 25, 1, DATEADD(day, -380, GETUTCDATE())),
    ('Наушники Sony WH-1000XM5', 'AUDIO-SONY-WH1000XM5', 'Аудио', 29999.99, 75, 1, DATEADD(day, -360, GETUTCDATE())),
    ('Планшет iPad Pro 12.9', 'TABLET-APL-IPADPRO-129', 'Планшеты', 99999.99, 30, 1, DATEADD(day, -340, GETUTCDATE())),
    ('Умные часы Apple Watch Series 9', 'WATCH-APL-AW9', 'Носимые устройства', 39999.99, 60, 1, DATEADD(day, -320, GETUTCDATE())),
    ('Игровая консоль PlayStation 5', 'CONSOLE-SONY-PS5', 'Игры', 54999.99, 20, 1, DATEADD(day, -300, GETUTCDATE())),
    ('Фитнес-браслет Xiaomi Mi Band 8', 'FITNESS-XIA-MB8', 'Фитнес', 3999.99, 100, 1, DATEADD(day, -280, GETUTCDATE())),
    ('Bluetooth колонка JBL Charge 5', 'AUDIO-JBL-CHARGE5', 'Аудио', 12999.99, 80, 1, DATEADD(day, -260, GETUTCDATE())),
    ('Веб-камера Logitech C920', 'CAMERA-LOG-C920', 'Периферия', 7999.99, 45, 1, DATEADD(day, -240, GETUTCDATE())),
    ('SSD Samsung 980 PRO 1TB', 'STORAGE-SAM-980PRO-1TB', 'Хранение', 8999.99, 70, 1, DATEADD(day, -220, GETUTCDATE())),
    ('Мышь Logitech MX Master 3', 'MOUSE-LOG-MXM3', 'Периферия', 6999.99, 55, 1, DATEADD(day, -200, GETUTCDATE())),
    ('Клавиатура Keychron K2', 'KEYBOARD-KEY-K2', 'Периферия', 9999.99, 40, 1, DATEADD(day, -180, GETUTCDATE())),
    ('Монитор LG 27UP850', 'MONITOR-LG-27UP850', 'Мониторы', 45999.99, 15, 1, DATEADD(day, -160, GETUTCDATE())),
    ('Роутер ASUS AX6000', 'NETWORK-ASUS-AX6000', 'Сеть', 19999.99, 35, 1, DATEADD(day, -140, GETUTCDATE())),
    ('Внешний аккумулятор Anker 20000mAh', 'POWER-ANK-20000', 'Аксессуары', 4999.99, 90, 1, DATEADD(day, -120, GETUTCDATE()))

PRINT CONCAT('Добавлено товаров: ', @@ROWCOUNT)

-- =======================================================
-- ЗАПОЛНЕНИЕ ТАБЛИЦЫ ORDERS
-- =======================================================

PRINT 'Заполнение таблицы Orders...'

-- Генерируем заказы с разными статусами и датами
DECLARE @OrderCounter INT = 1
DECLARE @CustomerId INT
DECLARE @OrderDate DATETIME2
DECLARE @Status NVARCHAR(20)

WHILE @OrderCounter <= 25
BEGIN
    -- Случайный клиент
    SET @CustomerId = (SELECT TOP 1 CustomerId FROM [demo].[Customers] ORDER BY NEWID())
    
    -- Случайная дата в последние 6 месяцев
    SET @OrderDate = DATEADD(day, -ABS(CHECKSUM(NEWID()) % 180), GETUTCDATE())
    
    -- Случайный статус
    SET @Status = CASE (ABS(CHECKSUM(NEWID()) % 5))
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Processing'
        WHEN 2 THEN 'Shipped'
        WHEN 3 THEN 'Delivered'
        WHEN 4 THEN 'Cancelled'
    END
    
    INSERT INTO [demo].[Orders] ([CustomerId], [OrderNumber], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [CreatedDate])
    VALUES (
        @CustomerId,
        CONCAT('ORD-', YEAR(@OrderDate), '-', FORMAT(@OrderCounter, '000000')),
        @OrderDate,
        0, -- Будет обновлено после добавления товаров
        @Status,
        CONCAT('г. Москва, ул. Примерная, д. ', ABS(CHECKSUM(NEWID()) % 100) + 1),
        @OrderDate
    )
    
    SET @OrderCounter = @OrderCounter + 1
END

PRINT CONCAT('Добавлено заказов: ', @@ROWCOUNT)

-- =======================================================
-- ЗАПОЛНЕНИЕ ТАБЛИЦЫ ORDERITEMS
-- =======================================================

PRINT 'Заполнение таблицы OrderItems...'

-- Добавляем товары к заказам
DECLARE order_cursor CURSOR FOR 
SELECT OrderId FROM [demo].[Orders]

DECLARE @OrderId INT
DECLARE @ProductId INT
DECLARE @Quantity INT
DECLARE @UnitPrice DECIMAL(10,2)
DECLARE @ItemsCount INT

OPEN order_cursor
FETCH NEXT FROM order_cursor INTO @OrderId

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Случайное количество товаров в заказе (1-5)
    SET @ItemsCount = ABS(CHECKSUM(NEWID()) % 5) + 1
    
    DECLARE @ItemCounter INT = 1
    WHILE @ItemCounter <= @ItemsCount
    BEGIN
        -- Случайный товар
        SELECT TOP 1 @ProductId = ProductId, @UnitPrice = Price 
        FROM [demo].[Products] 
        WHERE IsActive = 1
        ORDER BY NEWID()
        
        -- Случайное количество (1-3)
        SET @Quantity = ABS(CHECKSUM(NEWID()) % 3) + 1
        
        -- Проверяем, что товар еще не добавлен в этот заказ
        IF NOT EXISTS (SELECT 1 FROM [demo].[OrderItems] WHERE OrderId = @OrderId AND ProductId = @ProductId)
        BEGIN
            INSERT INTO [demo].[OrderItems] ([OrderId], [ProductId], [Quantity], [UnitPrice])
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)
        END
        
        SET @ItemCounter = @ItemCounter + 1
    END
    
    FETCH NEXT FROM order_cursor INTO @OrderId
END

CLOSE order_cursor
DEALLOCATE order_cursor

PRINT CONCAT('Добавлено позиций заказов: ', (SELECT COUNT(*) FROM [demo].[OrderItems]))

-- =======================================================
-- ОБНОВЛЕНИЕ ОБЩЕЙ СУММЫ ЗАКАЗОВ
-- =======================================================

PRINT 'Обновление общей суммы заказов...'

UPDATE o
SET TotalAmount = oi.TotalOrderAmount
FROM [demo].[Orders] o
INNER JOIN (
    SELECT 
        OrderId,
        SUM(Quantity * UnitPrice) AS TotalOrderAmount
    FROM [demo].[OrderItems]
    GROUP BY OrderId
) oi ON o.OrderId = oi.OrderId

PRINT CONCAT('Обновлено заказов: ', @@ROWCOUNT)

-- =======================================================
-- ЗАПОЛНЕНИЕ ТАБЛИЦЫ USERACTIONS
-- =======================================================

PRINT 'Заполнение таблицы UserActions...'

-- Генерируем действия пользователей
INSERT INTO [demo].[UserActions] ([UserId], [ActionType], [EntityType], [EntityId], [Description], [ActionDate], [IPAddress], [UserAgent])
SELECT 
    c.CustomerId,
    CASE (ABS(CHECKSUM(NEWID()) % 6))
        WHEN 0 THEN 'LOGIN'
        WHEN 1 THEN 'VIEW_PRODUCT'
        WHEN 2 THEN 'ADD_TO_CART'
        WHEN 3 THEN 'PLACE_ORDER'
        WHEN 4 THEN 'UPDATE_PROFILE'
        WHEN 5 THEN 'LOGOUT'
    END,
    CASE (ABS(CHECKSUM(NEWID()) % 3))
        WHEN 0 THEN 'Customer'
        WHEN 1 THEN 'Product'
        WHEN 2 THEN 'Order'
    END,
    ABS(CHECKSUM(NEWID()) % 100) + 1,
    'Автоматически сгенерированное действие пользователя',
    DATEADD(minute, -ABS(CHECKSUM(NEWID()) % 43200), GETUTCDATE()), -- Последние 30 дней
    CONCAT('192.168.1.', ABS(CHECKSUM(NEWID()) % 254) + 1),
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
FROM [demo].[Customers] c
CROSS JOIN (SELECT TOP 5 1 AS dummy FROM sys.objects) multiplier -- 5 действий на клиента

PRINT CONCAT('Добавлено действий пользователей: ', @@ROWCOUNT)

-- =======================================================
-- СОЗДАНИЕ ДОПОЛНИТЕЛЬНЫХ ДАННЫХ ДЛЯ ДЕМОНСТРАЦИИ CDC
-- =======================================================

PRINT 'Создание дополнительных изменений для демонстрации CDC...'

-- Обновляем некоторых клиентов
UPDATE [demo].[Customers] 
SET Phone = '+7-900-999-' + FORMAT(ABS(CHECKSUM(NEWID()) % 9999), '0000')
WHERE CustomerId IN (1, 3, 5)

-- Обновляем цены некоторых товаров
UPDATE [demo].[Products]
SET Price = Price * 1.1 -- Увеличиваем на 10%
WHERE ProductId IN (2, 4, 6, 8)

-- Меняем статусы некоторых заказов
UPDATE [demo].[Orders]
SET Status = 'Shipped'
WHERE Status = 'Processing' AND OrderId % 2 = 0

-- Деактивируем один товар
UPDATE [demo].[Products]
SET IsActive = 0
WHERE ProductId = (SELECT TOP 1 ProductId FROM [demo].[Products] ORDER BY NEWID())

-- Добавляем нового клиента
INSERT INTO [demo].[Customers] ([FirstName], [LastName], [Email], [Phone], [DateOfBirth], [RegistrationDate], [IsActive])
VALUES ('Новый', 'Клиент', 'new.client@email.com', '+7-900-NEW-CLIENT', '1995-06-15', GETUTCDATE(), 1)

PRINT 'Дополнительные изменения созданы для демонстрации CDC'

-- =======================================================
-- СТАТИСТИКА СОЗДАННЫХ ДАННЫХ
-- =======================================================

PRINT ''
PRINT '======================================================='
PRINT 'СТАТИСТИКА СОЗДАННЫХ ДАННЫХ'
PRINT '======================================================='

SELECT 
    'Customers' AS TableName,
    COUNT(*) AS RecordCount,
    MIN(RegistrationDate) AS EarliestDate,
    MAX(RegistrationDate) AS LatestDate
FROM [demo].[Customers]

UNION ALL

SELECT 
    'Products' AS TableName,
    COUNT(*) AS RecordCount,
    MIN(CreatedDate) AS EarliestDate,
    MAX(CreatedDate) AS LatestDate
FROM [demo].[Products]

UNION ALL

SELECT 
    'Orders' AS TableName,
    COUNT(*) AS RecordCount,
    MIN(OrderDate) AS EarliestDate,
    MAX(OrderDate) AS LatestDate
FROM [demo].[Orders]

UNION ALL

SELECT 
    'OrderItems' AS TableName,
    COUNT(*) AS RecordCount,
    MIN(CreatedDate) AS EarliestDate,
    MAX(CreatedDate) AS LatestDate
FROM [demo].[OrderItems]

UNION ALL

SELECT 
    'UserActions' AS TableName,
    COUNT(*) AS RecordCount,
    MIN(ActionDate) AS EarliestDate,
    MAX(ActionDate) AS LatestDate
FROM [demo].[UserActions]

-- =======================================================
-- ПРИМЕРЫ ЗАПРОСОВ ДЛЯ ПРОВЕРКИ CDC
-- =======================================================

PRINT ''
PRINT '======================================================='
PRINT 'ПРИМЕРЫ ДАННЫХ'
PRINT '======================================================='

-- Показываем несколько записей из каждой таблицы
PRINT 'Первые 5 клиентов:'
SELECT TOP 5 CustomerId, FirstName, LastName, Email, RegistrationDate, IsActive
FROM [demo].[Customers]
ORDER BY CustomerId

PRINT ''
PRINT 'Первые 5 товаров:'
SELECT TOP 5 ProductId, ProductName, SKU, Category, Price, StockQuantity, IsActive
FROM [demo].[Products]
ORDER BY ProductId

PRINT ''
PRINT 'Первые 5 заказов:'
SELECT TOP 5 OrderId, CustomerId, OrderNumber, OrderDate, TotalAmount, Status
FROM [demo].[Orders]
ORDER BY OrderId

PRINT ''
PRINT '======================================================='
PRINT 'ЗАПОЛНЕНИЕ ДАННЫМИ ЗАВЕРШЕНО!'
PRINT '======================================================='
PRINT 'Теперь можно тестировать CDC Bridge с реальными данными'
PRINT 'Все изменения будут отслеживаться в CDC таблицах'

-- Показываем последние CDC записи (если есть)
IF EXISTS (SELECT 1 FROM cdc.change_tables WHERE source_object_id = OBJECT_ID('demo.Customers'))
BEGIN
    PRINT ''
    PRINT 'Последние изменения в таблице Customers (CDC):'
    SELECT TOP 10 
        __$operation,
        __$start_lsn,
        CustomerId,
        FirstName,
        LastName,
        Email
    FROM cdc.demo_Customers_CT
    ORDER BY __$start_lsn DESC
END

GO