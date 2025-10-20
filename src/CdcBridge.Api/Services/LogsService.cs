using CdcBridge.Api.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace CdcBridge.Api.Services;

/// <summary>
/// Сервис для работы с логами, хранящимися в SQLite.
/// </summary>
public class LogsService
{
    private readonly string _logDbPath;

    public LogsService(IConfiguration configuration)
    {
        _logDbPath = configuration.GetValue<string>("Logging:SqliteDbPath") ?? "data/logs.db";
    }

    /// <summary>
    /// Получает список логов с фильтрацией и пагинацией.
    /// </summary>
    public async Task<PagedResultDto<LogEntryDto>> GetLogsAsync(LogQueryDto query)
    {
        var connectionString = $"Data Source={_logDbPath}";
        
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        // Построение SQL запроса с фильтрами
        var whereConditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (!string.IsNullOrEmpty(query.Level))
        {
            whereConditions.Add("Level = @Level");
            parameters.Add(new SqliteParameter("@Level", query.Level));
        }

        if (!string.IsNullOrEmpty(query.MessageSearch))
        {
            whereConditions.Add("Message LIKE @MessageSearch");
            parameters.Add(new SqliteParameter("@MessageSearch", $"%{query.MessageSearch}%"));
        }

        if (query.FromDate.HasValue)
        {
            whereConditions.Add("Timestamp >= @FromDate");
            parameters.Add(new SqliteParameter("@FromDate", query.FromDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        if (query.ToDate.HasValue)
        {
            whereConditions.Add("Timestamp <= @ToDate");
            parameters.Add(new SqliteParameter("@ToDate", query.ToDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Получаем общее количество записей
        var countQuery = $"SELECT COUNT(*) FROM Logs {whereClause}";
        using var countCommand = new SqliteCommand(countQuery, connection);
        countCommand.Parameters.AddRange(parameters.ToArray());
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

        // Получаем данные с пагинацией
        var offset = (query.Page - 1) * query.PageSize;
        var dataQuery = $@"
            SELECT Id, Timestamp, Level, Message, Exception, Properties 
            FROM Logs 
            {whereClause}
            ORDER BY Timestamp DESC 
            LIMIT @PageSize OFFSET @Offset";

        using var dataCommand = new SqliteCommand(dataQuery, connection);
        foreach (var param in parameters)
        {
            dataCommand.Parameters.Add(new SqliteParameter(param.ParameterName, param.Value));
        }
        dataCommand.Parameters.Add(new SqliteParameter("@PageSize", query.PageSize));
        dataCommand.Parameters.Add(new SqliteParameter("@Offset", offset));

        var logs = new List<LogEntryDto>();
        using var reader = await dataCommand.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            logs.Add(new LogEntryDto
            {
                Id = reader.GetInt32(0),
                Timestamp = DateTime.Parse(reader.GetString(1)),
                Level = reader.GetString(2),
                Message = reader.GetString(3),
                Exception = reader.IsDBNull(4) ? null : reader.GetString(4),
                Properties = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return new PagedResultDto<LogEntryDto>
        {
            Items = logs,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
