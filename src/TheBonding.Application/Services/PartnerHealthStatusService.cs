using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Services;

public sealed class PartnerHealthStatusService : IPartnerHealthStatusService
{
    private readonly IPartnerHealthStatusRepository _repo;
    private readonly IEncryptionService             _encryption;
    private readonly IAuthService                   _auth;

    public PartnerHealthStatusService(
        IPartnerHealthStatusRepository repo,
        IEncryptionService             encryption,
        IAuthService                   auth)
    {
        _repo       = repo;
        _encryption = encryption;
        _auth       = auth;
    }

    public async Task<Result<IReadOnlyList<PartnerHealthStatusSummary>>> GetByPartnerAsync(int partnerId)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<PartnerHealthStatusSummary>>("App is locked.");

        var statuses = await _repo.GetByPartnerAsync(partnerId);
        var result   = new List<PartnerHealthStatusSummary>(statuses.Count);

        foreach (var status in statuses)
        {
            var record = _encryption.DecryptBlob<PartnerHealthStatusRecord>(status.DataBlob, key2);
            if (record == null) continue;

            result.Add(new PartnerHealthStatusSummary
            {
                Id          = status.Id,
                PartnerId   = status.PartnerId,
                TestedDate  = status.TestedDate,
                CreatedDate = status.CreatedDate,
                Data        = record
            });
        }

        return Result.Success<IReadOnlyList<PartnerHealthStatusSummary>>(result);
    }

    public async Task<Result<PartnerHealthStatusSummary>> GetByIdAsync(int id)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<PartnerHealthStatusSummary>("App is locked.");

        var status = await _repo.GetByIdAsync(id);
        if (status == null)
            return Result.Failure<PartnerHealthStatusSummary>("Health status record not found.");

        var record = _encryption.DecryptBlob<PartnerHealthStatusRecord>(status.DataBlob, key2);
        if (record == null)
            return Result.Failure<PartnerHealthStatusSummary>("Failed to decrypt health status data.");

        return Result.Success(new PartnerHealthStatusSummary
        {
            Id          = status.Id,
            PartnerId   = status.PartnerId,
            TestedDate  = status.TestedDate,
            CreatedDate = status.CreatedDate,
            Data        = record
        });
    }

    public async Task<Result<int>> CreateAsync(int partnerId, PartnerHealthStatusRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<int>("App is locked.");

        if (string.IsNullOrWhiteSpace(record.TestedDate))
            return Result.Failure<int>("Tested date is required.");

        if (!DateTime.TryParse(record.TestedDate, out var testedDate))
            return Result.Failure<int>("Invalid tested date format.");

        var now = DateTime.UtcNow;
        var status = new PartnerHealthStatus
        {
            PartnerId   = partnerId,
            DataBlob    = _encryption.EncryptBlob(record, key2),
            TestedDate  = testedDate,
            CreatedDate = now
        };

        var id = await _repo.CreateAsync(status);
        return Result.Success(id);
    }

    public async Task<Result> UpdateAsync(int id, PartnerHealthStatusRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure("App is locked.");

        if (string.IsNullOrWhiteSpace(record.TestedDate))
            return Result.Failure("Tested date is required.");

        if (!DateTime.TryParse(record.TestedDate, out var testedDate))
            return Result.Failure("Invalid tested date format.");

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Health status record not found.");

        existing.DataBlob   = _encryption.EncryptBlob(record, key2);
        existing.TestedDate = testedDate;

        await _repo.UpdateAsync(existing);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return Result.Failure("Health status record not found.");

        await _repo.DeleteAsync(id);
        return Result.Success();
    }
}
