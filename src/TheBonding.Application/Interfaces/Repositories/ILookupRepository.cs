using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

public interface ILookupRepository
{
    // Categories
    Task<IReadOnlyList<LookupCategory>> GetCategoriesAsync();
    Task<LookupCategory?> GetCategoryByIdAsync(int id);
    Task<LookupCategory?> GetCategoryByNameAsync(string name);

    // Items
    Task<IReadOnlyList<LookupItem>> GetItemsByCategoryAsync(int categoryId);
    Task<LookupItem?> GetItemByIdAsync(int id);
    Task<int> CreateItemAsync(LookupItem item);
    Task UpdateItemAsync(LookupItem item);
    Task DeleteItemAsync(int id);
    Task<bool> ItemsExistForCategoryAsync(int categoryId);
}
