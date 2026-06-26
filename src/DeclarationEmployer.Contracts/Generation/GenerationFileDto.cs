namespace DeclarationEmployer.Contracts.Generation;

public sealed class GenerationFileDto
{
    public Guid GeneratedFileId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string? Sha256Hash { get; set; }

    public string FileType { get; set; } = string.Empty;
}
