using TheBonding.Application.Results;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Interfaces.Services;

public interface IUserHealthStatusService
{
    Task<Result<IReadOnlyList<UserHealthStatusSummary>>> GetAllAsync();
    Task<Result<UserHealthStatusSummary>> GetByIdAsync(int id);
    Task<Result<int>> CreateAsync(PartnerHealthStatusRecord record);
    Task<Result> UpdateAsync(int id, PartnerHealthStatusRecord record);
    Task<Result> DeleteAsync(int id);
}
