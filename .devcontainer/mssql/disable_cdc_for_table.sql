EXEC sys.sp_cdc_disable_table @source_schema = N'dbo', @source_name = N'TableName', @capture_instance = N'all'

exec sp_executesql N'DECLARE @SchemaID INT = SCHEMA_ID(''dbo'')
SELECT TOP 1 is_tracked_by_cdc FROM sys.tables
WHERE schema_id = @SchemaID AND name = @TableName',N'@TableName nvarchar(4000)',@TableName=N'TableName'