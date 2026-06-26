namespace DeclarationEmployer.Contracts.Declarations;

public sealed class ArchivedDocumentDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public Guid ClientCompanyId { get; set; }

    public Guid FiscalYearId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public DateTimeOffset ArchivedAt { get; set; }

    public string? ArchivedBy { get; set; }
}
