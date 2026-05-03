using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class AppSettingsRepository : IAppSettingsRepository
{
    private readonly DbConnectionFactory _factory;

    public AppSettingsRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<AppSettings?> GetAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Salt, AuthVerifier, EncryptedKey2,
                   FailedAttempts, LockoutUntil, IsSetup
            FROM AppSettings
            WHERE Id = 1;
            """;

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new AppSettings
        {
            Id             = reader.GetInt32(reader.GetOrdinal("Id")),
            Salt           = reader.GetString(reader.GetOrdinal("Salt")),
            AuthVerifier   = reader.GetString(reader.GetOrdinal("AuthVerifier")),
            EncryptedKey2  = reader.GetString(reader.GetOrdinal("EncryptedKey2")),
            FailedAttempts = reader.GetInt32(reader.GetOrdinal("FailedAttempts")),
            LockoutUntil   = ReadNullableDateTime(reader, "LockoutUntil"),
            IsSetup        = reader.GetInt32(reader.GetOrdinal("IsSetup")) == 1
        };
    }

    public async Task CreateAsync(AppSettings settings)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO AppSettings (Id, Salt, AuthVerifier, EncryptedKey2,
                                     FailedAttempts, LockoutUntil, IsSetup)
            VALUES (1, @salt, @authVerifier, @encryptedKey2,
                    0, NULL, @isSetup);
            """;
        cmd.Parameters.AddWithValue("@salt",          settings.Salt);
        cmd.Parameters.AddWithValue("@authVerifier",  settings.AuthVerifier);
        cmd.Parameters.AddWithValue("@encryptedKey2", settings.EncryptedKey2);
        cmd.Parameters.AddWithValue("@isSetup",       settings.IsSetup ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateFailedAttemptsAsync(int count, DateTime? lockoutUntil)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE AppSettings
            SET FailedAttempts = @count,
                LockoutUntil   = @lockoutUntil
            WHERE Id = 1;
            """;
        cmd.Parameters.AddWithValue("@count",        count);
        cmd.Parameters.AddWithValue("@lockoutUntil", (object?)lockoutUntil?.ToString("O") ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateCredentialsAsync(string salt, string authVerifier, string encryptedKey2)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE AppSettings
            SET Salt          = @salt,
                AuthVerifier  = @authVerifier,
                EncryptedKey2 = @encryptedKey2
            WHERE Id = 1;
            """;
        cmd.Parameters.AddWithValue("@salt",          salt);
        cmd.Parameters.AddWithValue("@authVerifier",  authVerifier);
        cmd.Parameters.AddWithValue("@encryptedKey2", encryptedKey2);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ResetFailedAttemptsAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE AppSettings
            SET FailedAttempts = 0,
                LockoutUntil   = NULL
            WHERE Id = 1;
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static DateTime? ReadNullableDateTime(Microsoft.Data.Sqlite.SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
