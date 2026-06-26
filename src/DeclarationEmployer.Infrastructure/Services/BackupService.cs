using System.Diagnostics;
using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Backup;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Backup;
using DeclarationEmployer.Domain.Backup;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class BackupService : IBackupService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileHashService _fileHashService;
    private readonly BackupOptions _options;

    public BackupService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IFileHashService fileHashService,
        IOptions<BackupOptions> options)
    {
        _db = db;
        _currentUserService = currentUserService;
        _fileHashService = fileHashService;
        _options = options.Value;
    }

    public async Task<BackupRecordDto> CreateAsync(
        CreateBackupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PgDumpPath) || !File.Exists(_options.PgDumpPath))
        {
            throw new ApplicationConflictException("Chemin pg_dump introuvable. Configurer Backup:PgDumpPath.");
        }

        Directory.CreateDirectory(_options.Directory);
        var fileName = $"det_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.dump";
        var fullPath = Path.Combine(_options.Directory, fileName);
        await RunPgDumpAsync(_options.PgDumpPath, fullPath, cancellationToken);

        var fileInfo = new FileInfo(fullPath);
        var hash = await _fileHashService.ComputeSha256Async(fullPath, cancellationToken);
        var record = new BackupRecord
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            StoredPath = fullPath,
            Sha256Hash = hash,
            SizeBytes = fileInfo.Length,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = _currentUserService.UserName,
            Status = BackupRecordStatus.Created,
            Notes = Normalize(request.Notes)
        };
        record.Events.Add(new BackupEvent
        {
            Id = Guid.NewGuid(),
            BackupRecordId = record.Id,
            Action = "BACKUP_CREATED",
            Description = "Sauvegarde PostgreSQL creee via pg_dump.",
            OccurredAt = DateTimeOffset.UtcNow
        });

        _db.BackupRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(record);
    }

    public async Task<IReadOnlyList<BackupRecordDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _db.BackupRecords
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(ToDto).ToList();
    }

    public async Task<BackupRecordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _db.BackupRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return record is null ? null : ToDto(record);
    }

    public async Task<BackupRecordDto> VerifyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _db.BackupRecords.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Sauvegarde introuvable.");

        if (!File.Exists(record.StoredPath))
        {
            record.Status = BackupRecordStatus.Failed;
            _db.BackupEvents.Add(new BackupEvent
            {
                Id = Guid.NewGuid(),
                BackupRecordId = record.Id,
                Action = "BACKUP_VERIFY_FAILED",
                Description = "Fichier de sauvegarde introuvable.",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(cancellationToken);
            throw new ApplicationConflictException("Fichier de sauvegarde introuvable.");
        }

        var hash = await _fileHashService.ComputeSha256Async(record.StoredPath, cancellationToken);
        if (!string.Equals(hash, record.Sha256Hash, StringComparison.OrdinalIgnoreCase))
        {
            record.Status = BackupRecordStatus.Failed;
            _db.BackupEvents.Add(new BackupEvent
            {
                Id = Guid.NewGuid(),
                BackupRecordId = record.Id,
                Action = "BACKUP_VERIFY_FAILED",
                Description = "Hash de sauvegarde incoherent.",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(cancellationToken);
            throw new ApplicationConflictException("Hash de sauvegarde incoherent.");
        }

        record.Status = BackupRecordStatus.Verified;
        _db.BackupEvents.Add(new BackupEvent
        {
            Id = Guid.NewGuid(),
            BackupRecordId = record.Id,
            Action = "BACKUP_VERIFIED",
            Description = "Sauvegarde verifiee.",
            OccurredAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(record);
    }

    private static async Task RunPgDumpAsync(string pgDumpPath, string fullPath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = pgDumpPath,
            Arguments = $"-Fc -f \"{fullPath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo) ?? throw new ApplicationConflictException("Impossible de demarrer pg_dump.");
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            throw new ApplicationConflictException($"pg_dump a echoue : {error}");
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static BackupRecordDto ToDto(BackupRecord record)
    {
        return new BackupRecordDto
        {
            Id = record.Id,
            FileName = record.FileName,
            StoredPath = record.StoredPath,
            Sha256Hash = record.Sha256Hash,
            SizeBytes = record.SizeBytes,
            CreatedAt = record.CreatedAt,
            CreatedBy = record.CreatedBy,
            Status = record.Status.ToString(),
            Notes = record.Notes
        };
    }
}
