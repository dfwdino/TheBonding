using TheBonding.Application.Results;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Interfaces.Services;

public interface IPartnerHealthStatusService
{
    Task<Result<IReadOnlyList<PartnerHealthStatusSummary>>> GetByPartnerAsync(int partnerId);
    Task<Result<PartnerHealthStatusSummary>> GetByIdAsync(int id);
    Task<Result<int>> CreateAsync(int partnerId, PartnerHealthStatusRecord record);
    Task<Result> UpdateAsync(int id, PartnerHealthStatusRecord record);
    Task<Result> DeleteAsync(int id);
}
