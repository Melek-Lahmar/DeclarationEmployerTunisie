namespace DeclarationEmployer.Contracts.Backup;

public sealed class BackupRecordDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredPath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
