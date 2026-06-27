namespace DeclarationEmployer.Contracts.Generation;

public sealed class EmpccaGeneratedArtifactDto
{
    public string FileName { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public int ExpectedLineLength { get; set; }
    public bool IsOfficial { get; set; }
    public IReadOnlyList<string> BlockingIssues { get; set; } = Array.Empty<string>();
}

public sealed class EmpccaGenerationPreviewDto
{
    public Guid DeclarationId { get; set; }
    public bool CanGenerateOfficial { get; set; }
    public IReadOnlyList<string> BlockingIssues { get; set; } = Array.Empty<string>();
    public IReadOnlyList<EmpccaGeneratedArtifactDto> Files { get; set; } = Array.Empty<EmpccaGeneratedArtifactDto>();
}
