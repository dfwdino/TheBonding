using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Domain.Entities;
using TheBonding.Infrastructure.Data;

namespace TheBonding.Infrastructure.Repositories;

public sealed class LookupRepository : ILookupRepository
{
    private readonly DbConnectionFactory _factory;

    public LookupRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    // -------------------------------------------------------------------------
    // Categories
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<LookupCategory>> GetCategoriesAsync()
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, IsSystem FROM LookupCategory ORDER BY Id;";

        var results = new List<LookupCategory>();
        using var reader = await cmd.ExecuteReaderAsync();

        var idOrd       = reader.GetOrdinal("Id");
        var nameOrd     = reader.GetOrdinal("Name");
        var isSystemOrd = reader.GetOrdinal("IsSystem");

        while (await reader.ReadAsync())
        {
            results.Add(new LookupCategory
            {
                Id       = reader.GetInt32(idOrd),
                Name     = reader.GetString(nameOrd),
                IsSystem = reader.GetInt32(isSystemOrd) == 1
            });
        }

        return results;
    }

    public async Task<LookupCategory?> GetCategoryByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, IsSystem FROM LookupCategory WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new LookupCategory
        {
            Id       = reader.GetInt32(reader.GetOrdinal("Id")),
            Name     = reader.GetString(reader.GetOrdinal("Name")),
            IsSystem = reader.GetInt32(reader.GetOrdinal("IsSystem")) == 1
        };
    }

    public async Task<LookupCategory?> GetCategoryByNameAsync(string name)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, IsSystem FROM LookupCategory WHERE Name = @name;";
        cmd.Parameters.AddWithValue("@name", name);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new LookupCategory
        {
            Id       = reader.GetInt32(reader.GetOrdinal("Id")),
            Name     = reader.GetString(reader.GetOrdinal("Name")),
            IsSystem = reader.GetInt32(reader.GetOrdinal("IsSystem")) == 1
        };
    }

    // -------------------------------------------------------------------------
    // Items
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<LookupItem>> GetItemsByCategoryAsync(int categoryId)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, CategoryId, EncryptedValue, SortOrder, IsDefault
            FROM LookupItem
            WHERE CategoryId = @categoryId
            ORDER BY SortOrder, Id;
            """;
        cmd.Parameters.AddWithValue("@categoryId", categoryId);

        var results = new List<LookupItem>();
        using var reader = await cmd.ExecuteReaderAsync();

        var idOrd             = reader.GetOrdinal("Id");
        var categoryIdOrd     = reader.GetOrdinal("CategoryId");
        var encryptedValueOrd = reader.GetOrdinal("EncryptedValue");
        var sortOrderOrd      = reader.GetOrdinal("SortOrder");
        var isDefaultOrd      = reader.GetOrdinal("IsDefault");

        while (await reader.ReadAsync())
        {
            results.Add(new LookupItem
            {
                Id             = reader.GetInt32(idOrd),
                CategoryId     = reader.GetInt32(categoryIdOrd),
                EncryptedValue = reader.GetString(encryptedValueOrd),
                SortOrder      = reader.GetInt32(sortOrderOrd),
                IsDefault      = reader.GetInt32(isDefaultOrd) == 1
            });
        }

        return results;
    }

    public async Task<LookupItem?> GetItemByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, CategoryId, EncryptedValue, SortOrder, IsDefault
            FROM LookupItem
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new LookupItem
        {
            Id             = reader.GetInt32(reader.GetOrdinal("Id")),
            CategoryId     = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            EncryptedValue = reader.GetString(reader.GetOrdinal("EncryptedValue")),
            SortOrder      = reader.GetInt32(reader.GetOrdinal("SortOrder")),
            IsDefault      = reader.GetInt32(reader.GetOrdinal("IsDefault")) == 1
        };
    }

    public async Task<int> CreateItemAsync(LookupItem item)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO LookupItem (CategoryId, EncryptedValue, SortOrder, IsDefault)
            VALUES (@categoryId, @encryptedValue, @sortOrder, @isDefault);
            """;
        cmd.Parameters.AddWithValue("@categoryId",     item.CategoryId);
        cmd.Parameters.AddWithValue("@encryptedValue", item.EncryptedValue);
        cmd.Parameters.AddWithValue("@sortOrder",      item.SortOrder);
        cmd.Parameters.AddWithValue("@isDefault",      item.IsDefault ? 1 : 0);
        await cmd.ExecuteNonQueryAsync();

        using var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        return (int)(long)(await idCmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateItemAsync(LookupItem item)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE LookupItem
            SET EncryptedValue = @encryptedValue,
                SortOrder      = @sortOrder
            WHERE Id = @id;
            """;
        cmd.Parameters.AddWithValue("@encryptedValue", item.EncryptedValue);
        cmd.Parameters.AddWithValue("@sortOrder",      item.SortOrder);
        cmd.Parameters.AddWithValue("@id",             item.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteItemAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM LookupItem WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ItemsExistForCategoryAsync(int categoryId)
    {
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM LookupItem WHERE CategoryId = @categoryId;";
        cmd.Parameters.AddWithValue("@categoryId", categoryId);

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }
}
