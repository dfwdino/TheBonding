using TheBonding.Domain.Entities;

namespace TheBonding.Application.Interfaces.Repositories;

/// <summary>
/// Single-row table. Created once at first-launch setup.
/// FailedAttempts and LockoutUntil are the only fields updated after creation.
/// </summary>
public interface IAppSettingsRepository
{
    Task<AppSettings?> GetAsync();
    Task CreateAsync(AppSettings settings);
    Task UpdateFailedAttemptsAsync(int count, DateTime? lockoutUntil);
    Task ResetFailedAttemptsAsync();
    Task UpdateCredentialsAsync(string salt, string authVerifier, string encryptedKey2);
}
