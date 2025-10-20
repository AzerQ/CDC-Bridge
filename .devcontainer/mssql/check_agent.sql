-- Check if SQL Server Agent is running
SELECT 
    servicename,
    status,
    status_desc
FROM sys.dm_server_services
WHERE servicename LIKE '%Agent%';
GO

-- Check Agent XPs configuration
SELECT 
    name,
    value,
    value_in_use,
    description
FROM sys.configurations
WHERE name = 'Agent XPs';
GO