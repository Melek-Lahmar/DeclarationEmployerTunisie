namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationExportResultDto
{
    public Guid DeclarationId { get; set; }

    public Guid GeneratedFileId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public string FileType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string DeclarationStatus { get; set; } = string.Empty;

    public int ExportedLinesCount { get; set; }

    public decimal TotalGrossAmount { get; set; }

    public decimal TotalTaxableAmount { get; set; }

    public decimal TotalWithheldAmount { get; set; }
}
