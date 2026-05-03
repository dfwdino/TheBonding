using TheBonding.Application.Results;

namespace TheBonding.Application.Interfaces.Services;

public interface IDataManagementService
{
    /// <summary>
    /// Deletes all personal records (events, partners, health records, profile)
    /// while keeping the user account and lookup lists intact.
    /// </summary>
    Task<Result> ClearPersonalDataAsync();

    /// <summary>
    /// Wipes everything including the user account and all lookup items.
    /// The app returns to first-launch state and requires a new account setup.
    /// </summary>
    Task<Result> FullResetAsync();
}
