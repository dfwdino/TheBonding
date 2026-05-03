using TheBonding.Application.Results;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Interfaces.Services;

public interface IActivityService
{
    Task<Result<IReadOnlyList<ActivitySummary>>> GetAllAsync();
    Task<Result<IReadOnlyList<ActivitySummary>>> GetByPartnerAsync(int partnerId);
    Task<Result<ActivitySummary>> GetByIdAsync(int id);
    Task<Result<int>> CreateAsync(ActivityRecord record);
    Task<Result> UpdateAsync(int id, ActivityRecord record);
    Task<Result> DeleteAsync(int id);
}
