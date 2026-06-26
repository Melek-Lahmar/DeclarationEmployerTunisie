namespace DeclarationEmployer.Contracts.Declarations;

public sealed class GeneratedFileDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public string FileType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
}
