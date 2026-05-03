using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Services;

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repo;
    private readonly IEncryptionService     _encryption;
    private readonly IAuthService           _auth;

    public UserProfileService(
        IUserProfileRepository repo,
        IEncryptionService     encryption,
        IAuthService           auth)
    {
        _repo       = repo;
        _encryption = encryption;
        _auth       = auth;
    }

    public async Task<Result<UserProfileRecord>> GetAsync()
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<UserProfileRecord>("App is locked.");

        var profile = await _repo.GetAsync();

        // Profile is optional — return empty record if not yet created
        if (profile == null)
            return Result.Success<UserProfileRecord>(new UserProfileRecord());

        var record = _encryption.DecryptBlob<UserProfileRecord>(profile.DataBlob, key2);
        if (record == null)
            return Result.Failure<UserProfileRecord>("Failed to decrypt profile data.");

        return Result.Success(record);
    }

    public async Task<Result> SaveAsync(UserProfileRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure("App is locked.");

        var blob = _encryption.EncryptBlob(record, key2);
        await _repo.UpsertAsync(new UserProfile { DataBlob = blob });

        return Result.Success();
    }

    public Task<bool> ExistsAsync() => _repo.ExistsAsync();
}
