-- =======================================================
-- Скрипт для инициализации схемы базы данных
-- Создание тестовых таблиц для демонстрации CDC Bridge
-- =======================================================

-- Проверяем, что мы подключены к правильной базе данных
USE [CdcBridgeDemo]
GO

-- Если база данных не существует, создаем её
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CdcBridgeDemo')
BEGIN
    CREATE DATABASE [CdcBridgeDemo]
END
GO

USE [CdcBridgeDemo]
GO

-- Создаем схему для тестовых таблиц
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'demo')
BEGIN
    EXEC('CREATE SCHEMA [demo]')
END
GO

-- =======================================================
-- Таблица Customers (Клиенты)
-- =======================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
BEGIN
    CREATE TABLE [demo].[Customers] (
        [CustomerId] INT IDENTITY(1,1) PRIMARY KEY,
        [FirstName] NVARCHAR(50) NOT NULL,
        [LastName] NVARCHAR(50) NOT NULL,
        [Email] NVARCHAR(100) UNIQUE NOT NULL,
        [Phone] NVARCHAR(20),
        [DateOfBirth] DATE,
        [RegistrationDate] DATETIME2 DEFAULT GETUTCDATE(),
        [IsActive] BIT DEFAULT 1,
        [LastUpdated] DATETIME2 DEFAULT GETUTCDATE()
    )
END
GO

-- =======================================================
-- Таблица Orders (Заказы)
-- =======================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
BEGIN
    CREATE TABLE [demo].[Orders] (
        [OrderId] INT IDENTITY(1,1) PRIMARY KEY,
        [CustomerId] INT NOT NULL,
        [OrderNumber] NVARCHAR(20) UNIQUE NOT NULL,
        [OrderDate] DATETIME2 DEFAULT GETUTCDATE(),
        [TotalAmount] DECIMAL(10,2) NOT NULL,
        [Status] NVARCHAR(20) DEFAULT 'Pending',
        [ShippingAddress] NVARCHAR(255),
        [CreatedDate] DATETIME2 DEFAULT GETUTCDATE(),
        [LastUpdated] DATETIME2 DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_Orders_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [demo].[Customers]([CustomerId])
    )
END
GO

-- =======================================================
-- Таблица Products (Товары)
-- =======================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
BEGIN
    CREATE TABLE [demo].[Products] (
        [ProductId] INT IDENTITY(1,1) PRIMARY KEY,
        [ProductName] NVARCHAR(100) NOT NULL,
        [SKU] NVARCHAR(50) UNIQUE NOT NULL,
        [Category] NVARCHAR(50),
        [Price] DECIMAL(10,2) NOT NULL,
        [StockQuantity] INT DEFAULT 0,
        [IsActive] BIT DEFAULT 1,
        [CreatedDate] DATETIME2 DEFAULT GETUTCDATE(),
        [LastUpdated] DATETIME2 DEFAULT GETUTCDATE()
    )
END
GO

-- =======================================================
-- Таблица OrderItems (Позиции заказа)
-- =======================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
BEGIN
    CREATE TABLE [demo].[OrderItems] (
        [OrderItemId] INT IDENTITY(1,1) PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [Quantity] INT NOT NULL,
        [UnitPrice] DECIMAL(10,2) NOT NULL,
        [TotalPrice] AS ([Quantity] * [UnitPrice]) PERSISTED,
        [CreatedDate] DATETIME2 DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [demo].[Orders]([OrderId]),
        CONSTRAINT [FK_OrderItems_Products] FOREIGN KEY ([ProductId]) REFERENCES [demo].[Products]([ProductId])
    )
END
GO

-- =======================================================
-- Таблица UserActions (Действия пользователей) - для аудита
-- =======================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserActions' AND xtype='U')
BEGIN
    CREATE TABLE [demo].[UserActions] (
        [ActionId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT,
        [ActionType] NVARCHAR(50) NOT NULL,
        [EntityType] NVARCHAR(50) NOT NULL,
        [EntityId] INT,
        [Description] NVARCHAR(255),
        [ActionDate] DATETIME2 DEFAULT GETUTCDATE(),
        [IPAddress] NVARCHAR(45),
        [UserAgent] NVARCHAR(255)
    )
END
GO

-- =======================================================
-- Создание индексов для оптимизации производительности
-- =======================================================

-- Индексы для таблицы Customers
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Email')
    CREATE NONCLUSTERED INDEX [IX_Customers_Email] ON [demo].[Customers] ([Email])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_LastUpdated')
    CREATE NONCLUSTERED INDEX [IX_Customers_LastUpdated] ON [demo].[Customers] ([LastUpdated])
GO

-- Индексы для таблицы Orders
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_CustomerId')
    CREATE NONCLUSTERED INDEX [IX_Orders_CustomerId] ON [demo].[Orders] ([CustomerId])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_OrderDate')
    CREATE NONCLUSTERED INDEX [IX_Orders_OrderDate] ON [demo].[Orders] ([OrderDate])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_LastUpdated')
    CREATE NONCLUSTERED INDEX [IX_Orders_LastUpdated] ON [demo].[Orders] ([LastUpdated])
GO

-- Индексы для таблицы Products
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_SKU')
    CREATE NONCLUSTERED INDEX [IX_Products_SKU] ON [demo].[Products] ([SKU])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Category')
    CREATE NONCLUSTERED INDEX [IX_Products_Category] ON [demo].[Products] ([Category])
GO

-- Индексы для таблицы OrderItems
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
    CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId] ON [demo].[OrderItems] ([OrderId])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderItems_ProductId')
    CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductId] ON [demo].[OrderItems] ([ProductId])
GO

-- Индексы для таблицы UserActions
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserActions_UserId_ActionDate')
    CREATE NONCLUSTERED INDEX [IX_UserActions_UserId_ActionDate] ON [demo].[UserActions] ([UserId], [ActionDate])
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserActions_EntityType_EntityId')
    CREATE NONCLUSTERED INDEX [IX_UserActions_EntityType_EntityId] ON [demo].[UserActions] ([EntityType], [EntityId])
GO

-- =======================================================
-- Создание триггеров для автоматического обновления LastUpdated
-- =======================================================

-- Триггер для таблицы Customers
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Customers_UpdateLastUpdated')
BEGIN
    EXEC('
    CREATE TRIGGER [demo].[TR_Customers_UpdateLastUpdated]
    ON [demo].[Customers]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE [demo].[Customers]
        SET [LastUpdated] = GETUTCDATE()
        FROM [demo].[Customers] c
        INNER JOIN inserted i ON c.[CustomerId] = i.[CustomerId]
    END')
END
GO

-- Триггер для таблицы Orders
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Orders_UpdateLastUpdated')
BEGIN
    EXEC('
    CREATE TRIGGER [demo].[TR_Orders_UpdateLastUpdated]
    ON [demo].[Orders]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE [demo].[Orders]
        SET [LastUpdated] = GETUTCDATE()
        FROM [demo].[Orders] o
        INNER JOIN inserted i ON o.[OrderId] = i.[OrderId]
    END')
END
GO

-- Триггер для таблицы Products
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Products_UpdateLastUpdated')
BEGIN
    EXEC('
    CREATE TRIGGER [demo].[TR_Products_UpdateLastUpdated]
    ON [demo].[Products]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE [demo].[Products]
        SET [LastUpdated] = GETUTCDATE()
        FROM [demo].[Products] p
        INNER JOIN inserted i ON p.[ProductId] = i.[ProductId]
    END')
END
GO

PRINT 'Схема базы данных успешно инициализирована!'
PRINT 'Созданы таблицы: Customers, Orders, Products, OrderItems, UserActions'
PRINT 'Созданы индексы и триггеры для оптимизации производительности'

-- Проверка созданных таблиц
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'demo'
ORDER BY TABLE_NAME;

GO