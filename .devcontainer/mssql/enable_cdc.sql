-- Enable CDC on the database
-- Note: This requires SQL Server Agent to be running

USE ApplicationDB;
GO

-- Enable CDC on the database
EXEC sys.sp_cdc_enable_db;
GO

-- Check if CDC is enabled
SELECT name, is_cdc_enabled 
FROM sys.databases 
WHERE name = 'ApplicationDB';
GO

-- Show CDC related jobs (these require SQL Server Agent)
SELECT 
    job_id,
    name,
    enabled,
    description
FROM msdb.dbo.sysjobs
WHERE name LIKE '%cdc%'
   OR description LIKE '%cdc%'
   OR description LIKE '%Change Data Capture%';
GO