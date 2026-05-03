using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

public interface IUserHealthStatusRepository
{
    Task<IReadOnlyList<UserHealthStatus>> GetAllAsync();
    Task<UserHealthStatus?> GetByIdAsync(int id);
    Task<int> CreateAsync(UserHealthStatus status);
    Task UpdateAsync(UserHealthStatus status);
    Task DeleteAsync(int id);
}
