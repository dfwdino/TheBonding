using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Services;

public sealed class PartnerService : IPartnerService
{
    private readonly IPartnerRepository             _repo;
    private readonly IPartnerHealthStatusRepository _healthRepo;
    private readonly IEncryptionService             _encryption;
    private readonly IAuthService                   _auth;

    public PartnerService(
        IPartnerRepository             repo,
        IPartnerHealthStatusRepository healthRepo,
        IEncryptionService             encryption,
        IAuthService                   auth)
    {
        _repo       = repo;
        _healthRepo = healthRepo;
        _encryption = encryption;
        _auth       = auth;
    }

    public async Task<Result<IReadOnlyList<PartnerSummary>>> GetAllAsync()
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<PartnerSummary>>("App is locked.");

        var partners = await _repo.GetAllAsync();
        return Result.Success<IReadOnlyList<PartnerSummary>>(Decrypt(partners, key2));
    }

    public async Task<Result<IReadOnlyList<PartnerSummary>>> GetActiveAsync()
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<PartnerSummary>>("App is locked.");

        var partners = await _repo.GetActiveAsync();
        return Result.Success<IReadOnlyList<PartnerSummary>>(Decrypt(partners, key2));
    }

    public async Task<Result<PartnerSummary>> GetByIdAsync(int id)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<PartnerSummary>("App is locked.");

        var partner = await _repo.GetByIdAsync(id);
        if (partner == null)
            return Result.Failure<PartnerSummary>("Partner not found.");

        var record = _encryption.DecryptBlob<PartnerRecord>(partner.DataBlob, key2);
        if (record == null)
            return Result.Failure<PartnerSummary>("Failed to decrypt partner data.");

        return Result.Success(ToSummary(partner, record));
    }

    public async Task<Result<int>> CreateAsync(PartnerRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<int>("App is locked.");

        if (string.IsNullOrWhiteSpace(record.Name))
            return Result.Failure<int>("Partner name is required.");

        var now = DateTime.UtcNow;
        var partner = new Partner
        {
            DataBlob     = _encryption.EncryptBlob(record, key2),
            IsActive     = true,
            LastUsedDate = now,
            CreatedDate  = now
        };

        var id = await _repo.CreateAsync(partner);
        return Result.Success(id);
    }

    public async Task<Result> UpdateAsync(int id, PartnerRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure("App is locked.");

        if (string.IsNullOrWhiteSpace(record.Name))
            return Result.Failure("Partner name is required.");

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Partner not found.");

        existing.DataBlob = _encryption.EncryptBlob(record, key2);
        await _repo.UpdateAsync(existing);

        return Result.Success();
    }

    public async Task<Result> SetActiveAsync(int id, bool isActive)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Partner not found.");

        await _repo.SetActiveAsync(id, isActive);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Partner not found.");

        if (await _repo.HasActivitiesAsync(id))
            return Result.Failure("This partner has linked activities. Mark them as inactive instead of deleting.");

        // Cascade delete health status records first
        await _healthRepo.DeleteByPartnerAsync(id);
        await _repo.DeleteAsync(id);

        return Result.Success();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private List<PartnerSummary> Decrypt(IReadOnlyList<Partner> partners, byte[] key2)
    {
        var result = new List<PartnerSummary>(partners.Count);
        foreach (var partner in partners)
        {
            var record = _encryption.DecryptBlob<PartnerRecord>(partner.DataBlob, key2);
            if (record == null) continue;   // Skip corrupted rows silently
            result.Add(ToSummary(partner, record));
        }
        return result;
    }

    private static PartnerSummary ToSummary(Partner partner, PartnerRecord record) => new()
    {
        Id           = partner.Id,
        IsActive     = partner.IsActive,
        LastUsedDate = partner.LastUsedDate,
        CreatedDate  = partner.CreatedDate,
        Data         = record
    };
}
