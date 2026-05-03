namespace TheBonding.Application.Interfaces.Services;

/// <summary>
/// Seeds default lookup item values on first-launch setup.
/// Called by IAuthService.SetupAsync after Key2 is available.
///
/// Default items are encrypted and inserted for all system categories.
/// Users can edit or delete any of these defaults after setup.
/// </summary>
public interface ILookupSeederService
{
    Task SeedDefaultsAsync(byte[] key2);
}
