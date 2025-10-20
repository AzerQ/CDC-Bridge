CREATE DATABASE ApplicationDB;
GO

-- Enable SQL Server Agent
EXEC sp_configure 'Agent XPs', 1;
RECONFIGURE;
GO

-- Start SQL Server Agent service
EXEC xp_servicecontrol 'START', 'SQLServerAGENT';
GO