namespace DeclarationEmployer.Contracts.Archive;

public sealed class ArchiveDeclarationResultDto
{
    public Guid DeclarationId { get; set; }

    public Guid ArchivedDocumentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public string DeclarationStatus { get; set; } = string.Empty;

    public DateTimeOffset ArchivedAt { get; set; }
}
