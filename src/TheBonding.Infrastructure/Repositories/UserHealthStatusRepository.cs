using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class UserHealthStatusRepository : IUserHealthStatusRepository
{
    private readonly DbConnectionFactory _factory;

    public UserHealthStatusRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<UserHealthStatus>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, DataBlob, TestedDate, CreatedDate
            FROM UserHealthStatus
            ORDER BY TestedDate DESC;
            """;

        var results = new List<UserHealthStatus>();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new UserHealthStatus
            {
                Id          = reader.GetInt32(reader.GetOrdinal("Id")),
                DataBlob    = reader.GetString(reader.GetOrdinal("DataBlob")),
                TestedDate  = reader.GetDateTime(reader.GetOrdinal("TestedDate")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
            });
        }

        return results;
    }

    public async Task<UserHealthStatus?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, DataBlob, TestedDate, CreatedDate
            FROM UserHealthStatus WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new UserHealthStatus
        {
            Id          = reader.GetInt32(reader.GetOrdinal("Id")),
            DataBlob    = reader.GetString(reader.GetOrdinal("DataBlob")),
            TestedDate  = reader.GetDateTime(reader.GetOrdinal("TestedDate")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
        };
    }

    public async Task<int> CreateAsync(UserHealthStatus status)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO UserHealthStatus (DataBlob, TestedDate, CreatedDate)
            VALUES (@dataBlob, @testedDate, @createdDate);
            """;
        cmd.Parameters.AddWithValue("@dataBlob",    status.DataBlob);
        cmd.Parameters.AddWithValue("@testedDate",  status.TestedDate.ToString("O"));
        cmd.Parameters.AddWithValue("@createdDate", status.CreatedDate.ToString("O"));
        await cmd.ExecuteNonQueryAsync();

        using var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        return (int)(long)(await idCmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(UserHealthStatus status)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE UserHealthStatus
            SET DataBlob = @dataBlob, TestedDate = @testedDate
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@dataBlob",   status.DataBlob);
        cmd.Parameters.AddWithValue("@testedDate", status.TestedDate.ToString("O"));
        cmd.Parameters.AddWithValue("@id",         status.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM UserHealthStatus WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
