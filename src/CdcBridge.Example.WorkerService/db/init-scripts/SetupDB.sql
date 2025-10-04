CREATE DATABASE mssql_cdc_test
GO

USE mssql_cdc_test
GO

-- Employee TABLE
CREATE TABLE [dbo].[employee](
                                 [id] [uniqueidentifier] NOT NULL,
                                 [first_name] [varchar](50) NULL,
                                 [last_name] [varchar](50) NULL,
                                 [email] [varchar](100) NULL,
                                 [phone] [varchar](50) NULL,
                                 [department_id] [int] NULL,
                                 [salary] [decimal](10,2) NULL,
                                 [hire_date] [date] NULL,
                                 [is_active] [bit] DEFAULT 1,
                                 [created_at] [datetime] DEFAULT GETDATE(),
                                 [updated_at] [datetime] DEFAULT GETDATE(),
                                 PRIMARY KEY (id));
GO

-- Departments TABLE
CREATE TABLE [dbo].[department](
                                   [id] [int] IDENTITY(1,1) NOT NULL,
                                   [name] [varchar](100) NOT NULL,
                                   [budget] [decimal](15,2) NULL,
                                   [location] [varchar](100) NULL,
                                   [manager_id] [uniqueidentifier] NULL,
                                   [created_at] [datetime] DEFAULT GETDATE(),
                                   PRIMARY KEY (id));
GO

-- Projects TABLE
CREATE TABLE [dbo].[project](
                                [id] [uniqueidentifier] NOT NULL,
                                [name] [varchar](200) NOT NULL,
                                [description] [text] NULL,
                                [start_date] [date] NULL,
                                [end_date] [date] NULL,
                                [budget] [decimal](15,2) NULL,
                                [status] [varchar](20) DEFAULT 'Active',
                                [department_id] [int] NULL,
                                [created_at] [datetime] DEFAULT GETDATE(),
                                PRIMARY KEY (id));
GO

-- Employee Projects (many-to-many)
CREATE TABLE [dbo].[employee_project](
                                         [employee_id] [uniqueidentifier] NOT NULL,
                                         [project_id] [uniqueidentifier] NOT NULL,
                                         [role] [varchar](50) NULL,
                                         [assigned_date] [date] DEFAULT GETDATE(),
                                         [hours_per_week] [int] NULL,
                                         PRIMARY KEY (employee_id, project_id));
GO

-- Add foreign keys
ALTER TABLE [dbo].[employee]
    ADD CONSTRAINT FK_Employee_Department
        FOREIGN KEY (department_id) REFERENCES department(id);
GO

ALTER TABLE [dbo].[department]
    ADD CONSTRAINT FK_Department_Manager
        FOREIGN KEY (manager_id) REFERENCES employee(id);
GO

ALTER TABLE [dbo].[project]
    ADD CONSTRAINT FK_Project_Department
        FOREIGN KEY (department_id) REFERENCES department(id);
GO

ALTER TABLE [dbo].[employee_project]
    ADD CONSTRAINT FK_EmployeeProject_Employee
        FOREIGN KEY (employee_id) REFERENCES employee(id);
GO

ALTER TABLE [dbo].[employee_project]
    ADD CONSTRAINT FK_EmployeeProject_Project
        FOREIGN KEY (project_id) REFERENCES project(id);
GO

-- Insert initial data
INSERT INTO [dbo].[department] ([name], [budget], [location])
VALUES
    ('IT', 500000.00, 'New York'),
    ('HR', 200000.00, 'London'),
    ('Finance', 350000.00, 'Tokyo'),
    ('Marketing', 300000.00, 'Berlin');
GO

INSERT INTO [dbo].[employee] ([id], [first_name], [last_name], [email], [phone], [department_id], [salary], [hire_date])
VALUES
    ('653f11df-ee89-4e17-ac01-d6542f007ea1', 'Rune', 'Jensen', 'rune.jensen@company.com', '+45 12345678', 1, 75000.00, '2020-01-15'),
    (NEWID(), 'Anna', 'Smith', 'anna.smith@company.com', '+44 98765432', 2, 65000.00, '2019-03-20'),
    (NEWID(), 'John', 'Doe', 'john.doe@company.com', '+81 55512345', 3, 80000.00, '2018-06-10');
GO

-- Update department managers
UPDATE d SET manager_id = e.id
FROM department d
         CROSS APPLY (
    SELECT TOP 1 id
    FROM employee
    WHERE department_id = d.id
    ORDER BY hire_date
) e;
GO

-- Insert projects
INSERT INTO [dbo].[project] ([id], [name], [description], [start_date], [end_date], [budget], [department_id])
VALUES
    (NEWID(), 'Website Redesign', 'Complete redesign of company website', '2024-01-01', '2024-06-30', 150000.00, 1),
    (NEWID(), 'HR System Upgrade', 'Upgrade HR management system', '2024-02-01', '2024-08-31', 100000.00, 2),
    (NEWID(), 'Financial Analytics', 'Implement new financial analytics platform', '2024-03-01', '2024-09-30', 200000.00, 3);
GO

-- Enable CDC (uncomment if needed)
-- EXEC sys.sp_cdc_enable_db;
-- GO

-- EXECUTE sys.sp_cdc_enable_table
--     @source_schema = N'dbo',
--     @source_name = N'employee',
--     @role_name = N'null',
--     @supports_net_changes = 1;
-- GO

-- EXECUTE sys.sp_cdc_enable_table
--     @source_schema = N'dbo',
--     @source_name = N'department',
--     @role_name = N'null',
--     @supports_net_changes = 1;
-- GO