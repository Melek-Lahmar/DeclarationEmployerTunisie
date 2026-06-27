namespace DeclarationEmployer.Domain.Declarations;

public sealed class GeneratedFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public GeneratedFileType FileType { get; set; } = GeneratedFileType.Other;

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public int? AnnexNumber { get; set; }

    public bool IsOfficial { get; set; }

    public string? ValidationStatus { get; set; }

    public int? LineCount { get; set; }

    public int? ExpectedLineLength { get; set; }

    public string? Encoding { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public EmployerDeclaration? Declaration { get; set; }
}
