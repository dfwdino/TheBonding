using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

public interface IActivityRepository
{
    Task<IReadOnlyList<Activity>> GetAllAsync();
    Task<IReadOnlyList<Activity>> GetByPartnerAsync(int partnerId);
    Task<Activity?> GetByIdAsync(int id);
    Task<int> CreateAsync(Activity activity);
    Task UpdateAsync(Activity activity);
    Task DeleteAsync(int id);
    Task<int> CountByPartnerAsync(int partnerId);
}
