#!/bin/bash
# SQL Server Agent management script

SApassword="P@ssw0rd"

case "$1" in
    start)
        echo "Starting SQL Server Agent..."
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SApassword -Q "EXEC xp_servicecontrol 'START', 'SQLServerAGENT';"
        ;;
    stop)
        echo "Stopping SQL Server Agent..."
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SApassword -Q "EXEC xp_servicecontrol 'STOP', 'SQLServerAGENT';"
        ;;
    status)
        echo "Checking SQL Server Agent status..."
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SApassword -i /workspaces/CDC-Bridge/.devcontainer/mssql/check_agent.sql
        ;;
    restart)
        echo "Restarting SQL Server Agent..."
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SApassword -Q "EXEC xp_servicecontrol 'STOP', 'SQLServerAGENT';"
        sleep 2
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SApassword -Q "EXEC xp_servicecontrol 'START', 'SQLServerAGENT';"
        ;;
    *)
        echo "Usage: $0 {start|stop|status|restart}"
        echo "  start   - Start SQL Server Agent"
        echo "  stop    - Stop SQL Server Agent"
        echo "  status  - Check SQL Server Agent status"
        echo "  restart - Restart SQL Server Agent"
        exit 1
        ;;
esac