using TheBonding.Application.Results;

namespace TheBonding.Application.Interfaces.Services;

/// <summary>
/// Manages the app's locked/unlocked state and all auth material.
///
/// Key2 is held in memory only while the app is unlocked.
/// All data services retrieve Key2 from this service via CurrentKey2.
///
/// Lockout rules: 5 failed attempts triggers a 10-minute lockout.
/// </summary>
public interface IAuthService
{
    /// <summary>True after first-launch setup has been completed.</summary>
    bool IsSetup { get; }

    /// <summary>True when the user has successfully unlocked the app.</summary>
    bool IsUnlocked { get; }

    /// <summary>
    /// The current Key2 held in memory. Null when the app is locked.
    /// Data services must check for null and return Failure if the app is locked.
    /// </summary>
    byte[]? CurrentKey2 { get; }

    /// <summary>
    /// Loads AppSettings from the database into memory.
    /// Call once at app startup before checking IsSetup or IsUnlocked.
    /// Safe to call multiple times.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// First-launch only. Derives Key1, generates Key2, stores auth material.
    /// Seeds default lookup items after setup.
    /// </summary>
    Task<Result> SetupAsync(string password);

    /// <summary>
    /// Verifies the password against AuthVerifier, decrypts Key2 into memory.
    /// Increments FailedAttempts on failure. Locks out after 5 failures.
    /// </summary>
    Task<Result> UnlockAsync(string password);

    /// <summary>Clears Key2 from memory. App returns to locked state.</summary>
    void Lock();

    /// <summary>Returns true if the app is currently in a lockout period.</summary>
    Task<bool> IsLockedOutAsync();

    /// <summary>Returns the lockout expiry time, or null if not locked out.</summary>
    Task<DateTime?> GetLockoutUntilAsync();
}
