using TheBonding.Application.Results;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Interfaces.Services;

public interface IPartnerService
{
    Task<Result<IReadOnlyList<PartnerSummary>>> GetAllAsync();
    Task<Result<IReadOnlyList<PartnerSummary>>> GetActiveAsync();
    Task<Result<PartnerSummary>> GetByIdAsync(int id);
    Task<Result<int>> CreateAsync(PartnerRecord record);
    Task<Result> UpdateAsync(int id, PartnerRecord record);

    /// <summary>
    /// Toggles a partner between active and inactive.
    /// Use instead of delete when the partner has linked activity history.
    /// </summary>
    Task<Result> SetActiveAsync(int id, bool isActive);

    /// <summary>
    /// Hard delete. Fails if the partner has linked activities — use SetActiveAsync instead.
    /// Also deletes all PartnerHealthStatus rows for this partner.
    /// </summary>
    Task<Result> DeleteAsync(int id);
}
