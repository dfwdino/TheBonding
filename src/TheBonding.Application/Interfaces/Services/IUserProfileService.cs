using TheBonding.Application.Results;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<Result<UserProfileRecord>> GetAsync();
    Task<Result> SaveAsync(UserProfileRecord record);
    Task<bool> ExistsAsync();
}
