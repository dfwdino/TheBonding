using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Services;

public sealed class LookupService : ILookupService
{
    private readonly ILookupRepository  _repo;
    private readonly IEncryptionService _encryption;
    private readonly IAuthService       _auth;

    public LookupService(
        ILookupRepository  repo,
        IEncryptionService encryption,
        IAuthService       auth)
    {
        _repo       = repo;
        _encryption = encryption;
        _auth       = auth;
    }

    public async Task<Result<IReadOnlyList<LookupCategory>>> GetCategoriesAsync()
    {
        var categories = await _repo.GetCategoriesAsync();
        return Result.Success<IReadOnlyList<LookupCategory>>(categories);
    }

    public async Task<Result<IReadOnlyList<LookupItemRecord>>> GetByCategoryNameAsync(string categoryName)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<LookupItemRecord>>("App is locked.");

        var category = await _repo.GetCategoryByNameAsync(categoryName);
        if (category == null)
            return Result.Failure<IReadOnlyList<LookupItemRecord>>($"Category '{categoryName}' not found.");

        return await GetDecryptedItemsAsync(category, key2);
    }

    public async Task<Result<IReadOnlyList<LookupItemRecord>>> GetByCategoryIdAsync(int categoryId)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<LookupItemRecord>>("App is locked.");

        var category = await _repo.GetCategoryByIdAsync(categoryId);
        if (category == null)
            return Result.Failure<IReadOnlyList<LookupItemRecord>>("Category not found.");

        return await GetDecryptedItemsAsync(category, key2);
    }

    public async Task<Result<int>> AddItemAsync(int categoryId, string value)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<int>("App is locked.");

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<int>("Value cannot be empty.");

        var category = await _repo.GetCategoryByIdAsync(categoryId);
        if (category == null)
            return Result.Failure<int>("Category not found.");

        var items = await _repo.GetItemsByCategoryAsync(categoryId);
        var nextSort = items.Count > 0 ? items.Max(i => i.SortOrder) + 1 : 0;

        var item = new LookupItem
        {
            CategoryId     = categoryId,
            EncryptedValue = _encryption.EncryptString(value.Trim(), key2),
            SortOrder      = nextSort,
            IsDefault      = false
        };

        var id = await _repo.CreateItemAsync(item);
        return Result.Success(id);
    }

    public async Task<Result> UpdateItemAsync(int id, string value, int sortOrder)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure("App is locked.");

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure("Value cannot be empty.");

        var existing = await _repo.GetItemByIdAsync(id);
        if (existing == null)
            return Result.Failure("Lookup item not found.");

        existing.EncryptedValue = _encryption.EncryptString(value.Trim(), key2);
        existing.SortOrder      = sortOrder;

        await _repo.UpdateItemAsync(existing);
        return Result.Success();
    }

    public async Task<Result> DeleteItemAsync(int id)
    {
        var existing = await _repo.GetItemByIdAsync(id);
        if (existing == null)
            return Result.Failure("Lookup item not found.");

        await _repo.DeleteItemAsync(id);
        return Result.Success();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<Result<IReadOnlyList<LookupItemRecord>>> GetDecryptedItemsAsync(
        LookupCategory category, byte[] key2)
    {
        var items  = await _repo.GetItemsByCategoryAsync(category.Id);
        var result = new List<LookupItemRecord>(items.Count);

        foreach (var item in items)
        {
            try
            {
                var value = _encryption.DecryptString(item.EncryptedValue, key2);
                result.Add(new LookupItemRecord
                {
                    Id           = item.Id,
                    CategoryId   = item.CategoryId,
                    CategoryName = category.Name,
                    Value        = value,
                    SortOrder    = item.SortOrder,
                    IsDefault    = item.IsDefault
                });
            }
            catch
            {
                // Skip items that fail to decrypt rather than crashing the whole list
            }
        }

        return Result.Success<IReadOnlyList<LookupItemRecord>>(result);
    }
}
