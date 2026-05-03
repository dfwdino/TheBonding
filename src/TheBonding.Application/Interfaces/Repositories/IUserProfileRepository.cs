using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

/// <summary>
/// Single-row table. Upsert handles both first save and subsequent updates.
/// </summary>
public interface IUserProfileRepository
{
    Task<UserProfile?> GetAsync();
    Task UpsertAsync(UserProfile profile);
    Task<bool> ExistsAsync();
}
