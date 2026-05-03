using Microsoft.Data.Sqlite;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Services;

public sealed class DataManagementService : IDataManagementService
{
    private readonly DbConnectionFactory _factory;

    public DataManagementService(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Result> ClearPersonalDataAsync()
    {
        try
        {
            using var connection = _factory.CreateConnection();
            await connection.OpenAsync();

            // Child rows before parent rows
            await ExecAsync(connection, "DELETE FROM Activity;");
            await ExecAsync(connection, "DELETE FROM PartnerHealthStatus;");
            await ExecAsync(connection, "DELETE FROM Partner;");
            await ExecAsync(connection, "DELETE FROM UserProfile;");
            await ExecAsync(connection, "DELETE FROM UserHealthStatus;");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to clear data: {ex.Message}");
        }
    }

    public async Task<Result> FullResetAsync()
    {
        try
        {
            using var connection = _factory.CreateConnection();
            await connection.OpenAsync();

            await ExecAsync(connection, "DELETE FROM Activity;");
            await ExecAsync(connection, "DELETE FROM PartnerHealthStatus;");
            await ExecAsync(connection, "DELETE FROM Partner;");
            await ExecAsync(connection, "DELETE FROM UserProfile;");
            await ExecAsync(connection, "DELETE FROM UserHealthStatus;");
            await ExecAsync(connection, "DELETE FROM LookupItem;");
            await ExecAsync(connection, "DELETE FROM AppSettings;");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to reset app: {ex.Message}");
        }
    }

    private static async Task ExecAsync(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
}
