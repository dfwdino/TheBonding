using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Application.Results;
using TheBonding.Domain.Entities;
using TheBonding.Shared.Models;

namespace TheBonding.Application.Services;

public sealed class UserHealthStatusService : IUserHealthStatusService
{
    private readonly IUserHealthStatusRepository _repo;
    private readonly IEncryptionService          _encryption;
    private readonly IAuthService                _auth;

    public UserHealthStatusService(
        IUserHealthStatusRepository repo,
        IEncryptionService          encryption,
        IAuthService                auth)
    {
        _repo       = repo;
        _encryption = encryption;
        _auth       = auth;
    }

    public async Task<Result<IReadOnlyList<UserHealthStatusSummary>>> GetAllAsync()
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<IReadOnlyList<UserHealthStatusSummary>>("App is locked.");

        var rows   = await _repo.GetAllAsync();
        var result = new List<UserHealthStatusSummary>(rows.Count);

        foreach (var row in rows)
        {
            var record = _encryption.DecryptBlob<PartnerHealthStatusRecord>(row.DataBlob, key2);
            if (record is null) continue;
            result.Add(new UserHealthStatusSummary
            {
                Id          = row.Id,
                TestedDate  = row.TestedDate,
                CreatedDate = row.CreatedDate,
                Data        = record
            });
        }

        return Result.Success<IReadOnlyList<UserHealthStatusSummary>>(result);
    }

    public async Task<Result<UserHealthStatusSummary>> GetByIdAsync(int id)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<UserHealthStatusSummary>("App is locked.");

        var row = await _repo.GetByIdAsync(id);
        if (row is null)
            return Result.Failure<UserHealthStatusSummary>("Record not found.");

        var record = _encryption.DecryptBlob<PartnerHealthStatusRecord>(row.DataBlob, key2);
        if (record is null)
            return Result.Failure<UserHealthStatusSummary>("Failed to decrypt record.");

        return Result.Success(new UserHealthStatusSummary
        {
            Id          = row.Id,
            TestedDate  = row.TestedDate,
            CreatedDate = row.CreatedDate,
            Data        = record
        });
    }

    public async Task<Result<int>> CreateAsync(PartnerHealthStatusRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure<int>("App is locked.");

        if (string.IsNullOrWhiteSpace(record.TestedDate))
            return Result.Failure<int>("Test date is required.");

        if (!DateTime.TryParse(record.TestedDate, out var testedDate))
            return Result.Failure<int>("Invalid date format.");

        var row = new UserHealthStatus
        {
            DataBlob    = _encryption.EncryptBlob(record, key2),
            TestedDate  = testedDate,
            CreatedDate = DateTime.UtcNow
        };

        var id = await _repo.CreateAsync(row);
        return Result.Success(id);
    }

    public async Task<Result> UpdateAsync(int id, PartnerHealthStatusRecord record)
    {
        if (_auth.CurrentKey2 is not { } key2)
            return Result.Failure("App is locked.");

        if (string.IsNullOrWhiteSpace(record.TestedDate))
            return Result.Failure("Test date is required.");

        if (!DateTime.TryParse(record.TestedDate, out var testedDate))
            return Result.Failure("Invalid date format.");

        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure("Record not found.");

        existing.DataBlob   = _encryption.EncryptBlob(record, key2);
        existing.TestedDate = testedDate;

        await _repo.UpdateAsync(existing);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return Result.Failure("Record not found.");

        await _repo.DeleteAsync(id);
        return Result.Success();
    }
}
