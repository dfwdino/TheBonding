using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class PartnerHealthStatusRepository : IPartnerHealthStatusRepository
{
    private readonly DbConnectionFactory _factory;

    public PartnerHealthStatusRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<PartnerHealthStatus>> GetByPartnerAsync(int partnerId)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PartnerId, DataBlob, TestedDate, CreatedDate
            FROM PartnerHealthStatus
            WHERE PartnerId = @partnerId
            ORDER BY TestedDate DESC;
            """;
        cmd.Parameters.AddWithValue("@partnerId", partnerId);

        var results = new List<PartnerHealthStatus>();
        using var reader = await cmd.ExecuteReaderAsync();

        var idOrd          = reader.GetOrdinal("Id");
        var partnerIdOrd   = reader.GetOrdinal("PartnerId");
        var dataBlobOrd    = reader.GetOrdinal("DataBlob");
        var testedDateOrd  = reader.GetOrdinal("TestedDate");
        var createdDateOrd = reader.GetOrdinal("CreatedDate");

        while (await reader.ReadAsync())
        {
            results.Add(new PartnerHealthStatus
            {
                Id          = reader.GetInt32(idOrd),
                PartnerId   = reader.GetInt32(partnerIdOrd),
                DataBlob    = reader.GetString(dataBlobOrd),
                TestedDate  = reader.GetDateTime(testedDateOrd),
                CreatedDate = reader.GetDateTime(createdDateOrd)
            });
        }

        return results;
    }

    public async Task<PartnerHealthStatus?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PartnerId, DataBlob, TestedDate, CreatedDate
            FROM PartnerHealthStatus
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new PartnerHealthStatus
        {
            Id          = reader.GetInt32(reader.GetOrdinal("Id")),
            PartnerId   = reader.GetInt32(reader.GetOrdinal("PartnerId")),
            DataBlob    = reader.GetString(reader.GetOrdinal("DataBlob")),
            TestedDate  = reader.GetDateTime(reader.GetOrdinal("TestedDate")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
        };
    }

    public async Task<int> CreateAsync(PartnerHealthStatus status)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO PartnerHealthStatus (PartnerId, DataBlob, TestedDate, CreatedDate)
            VALUES (@partnerId, @dataBlob, @testedDate, @createdDate);
            """;
        cmd.Parameters.AddWithValue("@partnerId",   status.PartnerId);
        cmd.Parameters.AddWithValue("@dataBlob",    status.DataBlob);
        cmd.Parameters.AddWithValue("@testedDate",  status.TestedDate.ToString("O"));
        cmd.Parameters.AddWithValue("@createdDate", status.CreatedDate.ToString("O"));
        await cmd.ExecuteNonQueryAsync();

        using var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        return (int)(long)(await idCmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(PartnerHealthStatus status)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE PartnerHealthStatus
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
        cmd.CommandText = "DELETE FROM PartnerHealthStatus WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteByPartnerAsync(int partnerId)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM PartnerHealthStatus WHERE PartnerId = @partnerId;";
        cmd.Parameters.AddWithValue("@partnerId", partnerId);

        await cmd.ExecuteNonQueryAsync();
    }
}
