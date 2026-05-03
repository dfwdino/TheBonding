using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

public interface IPartnerRepository
{
    Task<IReadOnlyList<Partner>> GetAllAsync();
    Task<IReadOnlyList<Partner>> GetActiveAsync();
    Task<Partner?> GetByIdAsync(int id);
    Task<int> CreateAsync(Partner partner);
    Task UpdateAsync(Partner partner);
    Task UpdateLastUsedDateAsync(int id, DateTime lastUsedDate);
    Task SetActiveAsync(int id, bool isActive);
    Task DeleteAsync(int id);

    /// <summary>
    /// Returns true if any Activity rows reference this partner.
    /// Used to prevent deleting a partner that has linked activity history.
    /// </summary>
    Task<bool> HasActivitiesAsync(int id);
}
