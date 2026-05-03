using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;

namespace TheBonding.Application.Services;

/// <summary>
/// Manages app locked/unlocked state and all cryptographic auth operations.
///
/// Startup sequence:
///   1. MauiProgram calls DatabaseInitializer.InitializeAsync().
///   2. MauiProgram resolves IAuthService and calls LoadAsync().
///   3. Blazor UI checks IsSetup to route to Setup or Unlock page.
///
/// Key lifecycle:
///   Key1 is derived from password, used transiently, never stored.
///   Key2 is stored encrypted. Decrypted into _key2 on successful unlock.
///   _key2 is zeroed and nulled on Lock().
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IAppSettingsRepository _settingsRepo;
    private readonly IEncryptionService     _encryption;
    private readonly ILookupSeederService   _seeder;

    private AppSettings? _cached;
    private byte[]?      _key2;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes    = 10;

    public AuthService(
        IAppSettingsRepository settingsRepo,
        IEncryptionService     encryption,
        ILookupSeederService   seeder)
    {
        _settingsRepo = settingsRepo;
        _encryption   = encryption;
        _seeder       = seeder;
    }

    public bool IsSetup    => _cached?.IsSetup == true;
    public bool IsUnlocked => _key2 != null;
    public byte[]? CurrentKey2 => _key2;

    // -------------------------------------------------------------------------
    // Startup
    // -------------------------------------------------------------------------

    public async Task LoadAsync()
    {
        _cached = await _settingsRepo.GetAsync();
    }

    // -------------------------------------------------------------------------
    // First-launch setup
    // -------------------------------------------------------------------------

    public async Task<Result> SetupAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure("Password cannot be empty.");

        // Reload to ensure we have the latest state
        _cached = await _settingsRepo.GetAsync();

        if (_cached?.IsSetup == true)
            return Result.Failure("App is already set up.");

        // Derive key material
        var salt         = _encryption.GenerateSalt();
        var key1         = _encryption.DeriveKey1(password, salt);
        var key2         = _encryption.GenerateKey2();
        var authVerifier = _encryption.ComputeAuthVerifier(key1);
        var encryptedKey2 = _encryption.EncryptKey2(key2, key1);

        var settings = new AppSettings
        {
            Salt          = Convert.ToBase64String(salt),
            AuthVerifier  = authVerifier,
            EncryptedKey2 = encryptedKey2,
            IsSetup       = true
        };

        await _settingsRepo.CreateAsync(settings);

        _cached = settings;
        _key2   = key2;

        // Seed encrypted default lookup values now that Key2 is available
        await _seeder.SeedDefaultsAsync(key2);

        return Result.Success();
    }

    // -------------------------------------------------------------------------
    // Unlock
    // -------------------------------------------------------------------------

    public async Task<Result> UnlockAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure("Password cannot be empty.");

        _cached = await _settingsRepo.GetAsync();

        if (_cached == null || !_cached.IsSetup)
            return Result.Failure("App has not been set up.");

        // Check if currently locked out
        if (_cached.LockoutUntil.HasValue && _cached.LockoutUntil > DateTime.UtcNow)
        {
            var minutes = (int)Math.Ceiling((_cached.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes);
            return Result.Failure($"Too many failed attempts. Try again in {minutes} minute(s).");
        }

        // Derive Key1 and verify against stored AuthVerifier
        var salt     = Convert.FromBase64String(_cached.Salt);
        var key1     = _encryption.DeriveKey1(password, salt);
        var verifier = _encryption.ComputeAuthVerifier(key1);

        if (!string.Equals(verifier, _cached.AuthVerifier, StringComparison.Ordinal))
        {
            var newCount    = _cached.FailedAttempts + 1;
            var lockoutUntil = newCount >= MaxFailedAttempts
                ? DateTime.UtcNow.AddMinutes(LockoutMinutes)
                : (DateTime?)null;

            await _settingsRepo.UpdateFailedAttemptsAsync(newCount, lockoutUntil);
            _cached.FailedAttempts = newCount;
            _cached.LockoutUntil   = lockoutUntil;

            if (lockoutUntil.HasValue)
                return Result.Failure($"Too many failed attempts. App locked for {LockoutMinutes} minutes.");

            var remaining = MaxFailedAttempts - newCount;
            return Result.Failure($"Incorrect password. {remaining} attempt(s) remaining before lockout.");
        }

        // Correct password — decrypt Key2 into memory
        _key2 = _encryption.DecryptKey2(_cached.EncryptedKey2, key1);

        await _settingsRepo.ResetFailedAttemptsAsync();
        _cached.FailedAttempts = 0;
        _cached.LockoutUntil   = null;

        // Seed any lookup categories that were added since first setup.
        // The seeder skips categories that already have items, so this is a no-op
        // for all but newly introduced categories.
        await _seeder.SeedDefaultsAsync(_key2);

        return Result.Success();
    }

    // -------------------------------------------------------------------------
    // Lock
    // -------------------------------------------------------------------------

    public void Lock()
    {
        if (_key2 != null)
        {
            Array.Clear(_key2, 0, _key2.Length);   // Zero key material before GC
            _key2 = null;
        }
    }

    // -------------------------------------------------------------------------
    // Lockout status
    // -------------------------------------------------------------------------

    public async Task<bool> IsLockedOutAsync()
    {
        var settings = await _settingsRepo.GetAsync();
        return settings?.LockoutUntil.HasValue == true
            && settings.LockoutUntil > DateTime.UtcNow;
    }

    public async Task<DateTime?> GetLockoutUntilAsync()
    {
        var settings = await _settingsRepo.GetAsync();
        return settings?.LockoutUntil;
    }
}
