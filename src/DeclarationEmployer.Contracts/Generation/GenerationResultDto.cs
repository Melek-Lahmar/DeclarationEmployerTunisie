namespace DeclarationEmployer.Contracts.Generation;

public sealed class GenerationResultDto
{
    public Guid DeclarationId { get; set; }

    public bool IsOfficialMode { get; set; }

    public string Message { get; set; } = string.Empty;

    public string DeclarationStatus { get; set; } = string.Empty;

    public IReadOnlyList<GenerationFileDto> Files { get; set; } = Array.Empty<GenerationFileDto>();
}
