namespace DeclarationEmployer.Domain.Backup;

public sealed class BackupEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BackupRecordId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public BackupRecord? BackupRecord { get; set; }
}
