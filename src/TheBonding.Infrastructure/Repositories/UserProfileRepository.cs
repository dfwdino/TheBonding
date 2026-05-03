using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly DbConnectionFactory _factory;

    public UserProfileRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<UserProfile?> GetAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, DataBlob FROM UserProfile WHERE Id = 1;";

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new UserProfile
        {
            Id       = reader.GetInt32(reader.GetOrdinal("Id")),
            DataBlob = reader.GetString(reader.GetOrdinal("DataBlob"))
        };
    }

    public async Task UpsertAsync(UserProfile profile)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO UserProfile (Id, DataBlob)
            VALUES (1, @dataBlob)
            ON CONFLICT(Id) DO UPDATE SET DataBlob = excluded.DataBlob;
            """;
        cmd.Parameters.AddWithValue("@dataBlob", profile.DataBlob);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM UserProfile WHERE Id = 1;";

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }
}
