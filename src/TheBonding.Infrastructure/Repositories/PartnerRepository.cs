using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class PartnerRepository : IPartnerRepository
{
    private readonly DbConnectionFactory _factory;

    public PartnerRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Partner>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, DataBlob, IsActive, LastUsedDate, CreatedDate
            FROM Partner
            ORDER BY LastUsedDate DESC;
            """;

        return await ReadPartnersAsync(cmd);
    }

    public async Task<IReadOnlyList<Partner>> GetActiveAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, DataBlob, IsActive, LastUsedDate, CreatedDate
            FROM Partner
            WHERE IsActive = 1
            ORDER BY LastUsedDate DESC;
            """;

        return await ReadPartnersAsync(cmd);
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, DataBlob, IsActive, LastUsedDate, CreatedDate
            FROM Partner
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return MapPartner(reader);
    }

    public async Task<int> CreateAsync(Partner partner)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Partner (DataBlob, IsActive, LastUsedDate, CreatedDate)
            VALUES (@dataBlob, @isActive, @lastUsedDate, @createdDate);
            """;
        cmd.Parameters.AddWithValue("@dataBlob",     partner.DataBlob);
        cmd.Parameters.AddWithValue("@isActive",     partner.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@lastUsedDate", partner.LastUsedDate.ToString("O"));
        cmd.Parameters.AddWithValue("@createdDate",  partner.CreatedDate.ToString("O"));
        await cmd.ExecuteNonQueryAsync();

        using var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        return (int)(long)(await idCmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Partner partner)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Partner
            SET DataBlob     = @dataBlob,
                IsActive     = @isActive,
                LastUsedDate = @lastUsedDate
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@dataBlob",     partner.DataBlob);
        cmd.Parameters.AddWithValue("@isActive",     partner.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@lastUsedDate", partner.LastUsedDate.ToString("O"));
        cmd.Parameters.AddWithValue("@id",           partner.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateLastUsedDateAsync(int id, DateTime lastUsedDate)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Partner
            SET LastUsedDate = @lastUsedDate
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@lastUsedDate", lastUsedDate.ToString("O"));
        cmd.Parameters.AddWithValue("@id",           id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Partner
            SET IsActive = @isActive
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@isActive", isActive ? 1 : 0);
        cmd.Parameters.AddWithValue("@id",       id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Partner WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> HasActivitiesAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Activity WHERE PartnerId = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static async Task<IReadOnlyList<Partner>> ReadPartnersAsync(Microsoft.Data.Sqlite.SqliteCommand cmd)
    {
        var results = new List<Partner>();
        using var reader = await cmd.ExecuteReaderAsync();

        var idOrd           = reader.GetOrdinal("Id");
        var dataBlobOrd     = reader.GetOrdinal("DataBlob");
        var isActiveOrd     = reader.GetOrdinal("IsActive");
        var lastUsedDateOrd = reader.GetOrdinal("LastUsedDate");
        var createdDateOrd  = reader.GetOrdinal("CreatedDate");

        while (await reader.ReadAsync())
        {
            results.Add(new Partner
            {
                Id           = reader.GetInt32(idOrd),
                DataBlob     = reader.GetString(dataBlobOrd),
                IsActive     = reader.GetInt32(isActiveOrd) == 1,
                LastUsedDate = reader.GetDateTime(lastUsedDateOrd),
                CreatedDate  = reader.GetDateTime(createdDateOrd)
            });
        }

        return results;
    }

    private static Partner MapPartner(Microsoft.Data.Sqlite.SqliteDataReader reader) => new()
    {
        Id           = reader.GetInt32(reader.GetOrdinal("Id")),
        DataBlob     = reader.GetString(reader.GetOrdinal("DataBlob")),
        IsActive     = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
        LastUsedDate = reader.GetDateTime(reader.GetOrdinal("LastUsedDate")),
        CreatedDate  = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
    };
}
