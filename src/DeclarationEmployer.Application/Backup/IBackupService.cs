using DeclarationEmployer.Contracts.Backup;

namespace DeclarationEmployer.Application.Backup;

public interface IBackupService
{
    Task<BackupRecordDto> CreateAsync(CreateBackupRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackupRecordDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<BackupRecordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BackupRecordDto> VerifyAsync(Guid id, CancellationToken cancellationToken = default);
}
