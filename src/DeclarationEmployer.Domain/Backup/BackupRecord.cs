namespace DeclarationEmployer.Domain.Backup;

public sealed class BackupRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = string.Empty;

    public string StoredPath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public BackupRecordStatus Status { get; set; } = BackupRecordStatus.Created;

    public string? Notes { get; set; }

    public ICollection<BackupEvent> Events { get; set; } = new List<BackupEvent>();
}
