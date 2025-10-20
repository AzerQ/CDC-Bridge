-- Declaring a variable and Setting to zero first
DECLARE @cleanup_failed_bit BIT = 0;
DECLARE @retcode NUMERIC;
DECLARE @LSN BIGINT;

SELECT @LSN = sys.fn_cdc_get_max_lsn()

-- Execute cleanup and obtain output bit
EXECUTE
    @retcode = sys.sp_cdc_cleanup_change_table
               @capture_instance = 'dbo_employee',
               @low_water_mark = @LSN, --== LSN to be used for new low watermark for capture instance
               @threshold = 1

SELECT @retcode

-- Leverage @cleanup_failed_bit output to check the status.
SELECT IIF (@retcode > 0, 'CLEANUP FAILURE', 'CLEANUP SUCCESS');