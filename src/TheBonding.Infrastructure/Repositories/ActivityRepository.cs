using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly DbConnectionFactory _factory;

    public ActivityRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Activity>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PartnerId, DataBlob, OccurredDate, CreatedDate
            FROM Activity
            ORDER BY OccurredDate DESC;
            """;

        return await ReadActivitiesAsync(cmd);
    }

    public async Task<IReadOnlyList<Activity>> GetByPartnerAsync(int partnerId)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PartnerId, DataBlob, OccurredDate, CreatedDate
            FROM Activity
            WHERE PartnerId = @partnerId
            ORDER BY OccurredDate DESC;
            """;
        cmd.Parameters.AddWithValue("@partnerId", partnerId);

        return await ReadActivitiesAsync(cmd);
    }

    public async Task<Activity?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, PartnerId, DataBlob, OccurredDate, CreatedDate
            FROM Activity
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return MapActivity(reader);
    }

    public async Task<int> CreateAsync(Activity activity)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Activity (PartnerId, DataBlob, OccurredDate, CreatedDate)
            VALUES (@partnerId, @dataBlob, @occurredDate, @createdDate);
            """;
        cmd.Parameters.AddWithValue("@partnerId",   (object?)activity.PartnerId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dataBlob",    activity.DataBlob);
        cmd.Parameters.AddWithValue("@occurredDate", activity.OccurredDate.ToString("O"));
        cmd.Parameters.AddWithValue("@createdDate", activity.CreatedDate.ToString("O"));
        await cmd.ExecuteNonQueryAsync();

        using var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        return (int)(long)(await idCmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Activity activity)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Activity
            SET PartnerId    = @partnerId,
                DataBlob     = @dataBlob,
                OccurredDate = @occurredDate
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@partnerId",    (object?)activity.PartnerId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dataBlob",     activity.DataBlob);
        cmd.Parameters.AddWithValue("@occurredDate", activity.OccurredDate.ToString("O"));
        cmd.Parameters.AddWithValue("@id",           activity.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Activity WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> CountByPartnerAsync(int partnerId)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Activity WHERE PartnerId = @partnerId;";
        cmd.Parameters.AddWithValue("@partnerId", partnerId);

        return (int)(long)(await cmd.ExecuteScalarAsync())!;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static async Task<IReadOnlyList<Activity>> ReadActivitiesAsync(Microsoft.Data.Sqlite.SqliteCommand cmd)
    {
        var results = new List<Activity>();
        using var reader = await cmd.ExecuteReaderAsync();

        var idOrd           = reader.GetOrdinal("Id");
        var partnerIdOrd    = reader.GetOrdinal("PartnerId");
        var dataBlobOrd     = reader.GetOrdinal("DataBlob");
        var occurredDateOrd = reader.GetOrdinal("OccurredDate");
        var createdDateOrd  = reader.GetOrdinal("CreatedDate");

        while (await reader.ReadAsync())
        {
            results.Add(MapActivity(reader, idOrd, partnerIdOrd, dataBlobOrd, occurredDateOrd, createdDateOrd));
        }

        return results;
    }

    private static Activity MapActivity(Microsoft.Data.Sqlite.SqliteDataReader reader) =>
        MapActivity(
            reader,
            reader.GetOrdinal("Id"),
            reader.GetOrdinal("PartnerId"),
            reader.GetOrdinal("DataBlob"),
            reader.GetOrdinal("OccurredDate"),
            reader.GetOrdinal("CreatedDate"));

    private static Activity MapActivity(
        Microsoft.Data.Sqlite.SqliteDataReader reader,
        int idOrd, int partnerIdOrd, int dataBlobOrd, int occurredDateOrd, int createdDateOrd) => new()
    {
        Id           = reader.GetInt32(idOrd),
        PartnerId    = reader.IsDBNull(partnerIdOrd) ? null : reader.GetInt32(partnerIdOrd),
        DataBlob     = reader.GetString(dataBlobOrd),
        OccurredDate = reader.GetDateTime(occurredDateOrd),
        CreatedDate  = reader.GetDateTime(createdDateOrd)
    };
}
