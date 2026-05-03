using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

public interface IPartnerHealthStatusRepository
{
    Task<IReadOnlyList<PartnerHealthStatus>> GetByPartnerAsync(int partnerId);
    Task<PartnerHealthStatus?> GetByIdAsync(int id);
    Task<int> CreateAsync(PartnerHealthStatus status);
    Task UpdateAsync(PartnerHealthStatus status);
    Task DeleteAsync(int id);
    Task DeleteByPartnerAsync(int partnerId);
}
