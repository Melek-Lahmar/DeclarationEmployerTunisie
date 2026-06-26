using DeclarationEmployer.Domain.Cabinet;

namespace DeclarationEmployer.Domain.Declarations;

public sealed class ArchivedDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public Guid ClientCompanyId { get; set; }

    public Guid FiscalYearId { get; set; }

    public ArchivedDocumentType DocumentType { get; set; } = ArchivedDocumentType.Other;

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public DateTimeOffset ArchivedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? ArchivedBy { get; set; }

    public EmployerDeclaration? Declaration { get; set; }

    public ClientCompany? ClientCompany { get; set; }

    public FiscalYear? FiscalYear { get; set; }
}
