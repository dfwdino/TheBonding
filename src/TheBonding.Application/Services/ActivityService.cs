using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Services;

public sealed class ActivityService : IActivityService
{
    private readonly IActivityRepository _repo;
    private readonly IPartnerRepository  _partnerRepo;
    private readonly IEncryptionService  _encryption;
    private readonly IAuthService        _auth;

    public ActivityService(
        IActivityRepository activityRepo,
        IPartnerRepository  partnerRepo,
        IEncryptionService  encryption,
        IAuthService        auth)
    {
        _repo        = activityRepo;
        _partnerRepo = partnerRepo;
        _encryption  = encryption;
        _auth        = auth;
    }

    public async Task<Result<IReadOnlyList<ActivitySummary>>> GetAllAsync()
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<ActivitySummary>>("App is locked.");

        var activities = await _repo.GetAllAsync();
        return Result.Success<IReadOnlyList<ActivitySummary>>(Decrypt(activities, key2));
    }

    public async Task<Result<IReadOnlyList<ActivitySummary>>> GetByPartnerAsync(int partnerId)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<ActivitySummary>>("App is locked.");

        var activities = await _repo.GetByPartnerAsync(partnerId);
        return Result.Success<IReadOnlyList<ActivitySummary>>(Decrypt(activities, key2));
    }

    public async Task<Result<ActivitySummary>> GetByIdAsync(int id)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<ActivitySummary>("App is locked.");

        var activity = await _repo.GetByIdAsync(id);
        if (activity == null)
            return Result.Failure<ActivitySummary>("Activity not found.");

        var record = _encryption.DecryptBlob<ActivityRecord>(activity.DataBlob, key2);
        if (record == null)
            return Result.Failure<ActivitySummary>("Failed to decrypt activity data.");

        return Result.Success(ToSummary(activity, record));
    }

    public async Task<Result<int>> CreateAsync(ActivityRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<int>("App is locked.");

        if (string.IsNullOrWhiteSpace(record.OccurredDate))
            return Result.Failure<int>("Occurred date is required.");

        if (!DateTime.TryParse(record.OccurredDate, out var occurredDate))
            return Result.Failure<int>("Invalid occurred date format.");

        var now = DateTime.UtcNow;
        // Primary partner: first in PartnerIds list, or legacy PartnerId for old records
        var primaryPartnerId = record.PartnerIds.Count > 0 ? record.PartnerIds[0] : record.PartnerId;

        var activity = new Activity
        {
            PartnerId    = primaryPartnerId,
            DataBlob     = _encryption.EncryptBlob(record, key2),
            OccurredDate = occurredDate,
            CreatedDate  = now
        };

        var id = await _repo.CreateAsync(activity);

        // Update LastUsedDate for all partners involved
        foreach (var pid in record.PartnerIds)
            await _partnerRepo.UpdateLastUsedDateAsync(pid, now);
        if (record.PartnerIds.Count == 0 && record.PartnerId.HasValue)
            await _partnerRepo.UpdateLastUsedDateAsync(record.PartnerId.Value, now);

        return Result.Success(id);
    }

    public async Task<Result> UpdateAsync(int id, ActivityRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure("App is locked.");

        if (string.IsNullOrWhiteSpace(record.OccurredDate))
            return Result.Failure("Occurred date is required.");

        if (!DateTime.TryParse(record.OccurredDate, out var occurredDate))
            return Result.Failure("Invalid occurred date format.");

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Activity not found.");

        var primaryPartnerId = record.PartnerIds.Count > 0 ? record.PartnerIds[0] : record.PartnerId;

        existing.PartnerId    = primaryPartnerId;
        existing.DataBlob     = _encryption.EncryptBlob(record, key2);
        existing.OccurredDate = occurredDate;

        await _repo.UpdateAsync(existing);

        // Update LastUsedDate for all partners involved
        var now = DateTime.UtcNow;
        foreach (var pid in record.PartnerIds)
            await _partnerRepo.UpdateLastUsedDateAsync(pid, now);
        if (record.PartnerIds.Count == 0 && record.PartnerId.HasValue)
            await _partnerRepo.UpdateLastUsedDateAsync(record.PartnerId.Value, now);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Activity not found.");

        await _repo.DeleteAsync(id);
        return Result.Success();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private List<ActivitySummary> Decrypt(IReadOnlyList<Activity> activities, byte[] key2)
    {
        var result = new List<ActivitySummary>(activities.Count);
        foreach (var activity in activities)
        {
            var record = _encryption.DecryptBlob<ActivityRecord>(activity.DataBlob, key2);
            if (record == null) continue;
            result.Add(ToSummary(activity, record));
        }
        return result;
    }

    private static ActivitySummary ToSummary(Activity activity, ActivityRecord record) => new()
    {
        Id           = activity.Id,
        PartnerId    = activity.PartnerId,
        OccurredDate = activity.OccurredDate,
        CreatedDate  = activity.CreatedDate,
        Data         = record
    };
}
