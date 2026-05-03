using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Interfaces.Services;

public interface ILookupService
{
    /// <summary>Returns all categories (used for the Manage Lookups screen).</summary>
    Task<Result<IReadOnlyList<LookupCategory>>> GetCategoriesAsync();

    /// <summary>Returns all decrypted items for a given category name.</summary>
    Task<Result<IReadOnlyList<LookupItemRecord>>> GetByCategoryNameAsync(string categoryName);

    /// <summary>Returns all decrypted items for a given category id.</summary>
    Task<Result<IReadOnlyList<LookupItemRecord>>> GetByCategoryIdAsync(int categoryId);

    /// <summary>Adds a new user-defined item to a category.</summary>
    Task<Result<int>> AddItemAsync(int categoryId, string value);

    /// <summary>Updates the decrypted value and sort order of an existing item.</summary>
    Task<Result> UpdateItemAsync(int id, string value, int sortOrder);

    /// <summary>Hard deletes a lookup item. Any references in activity/partner blobs remain (stored by id).</summary>
    Task<Result> DeleteItemAsync(int id);
}
