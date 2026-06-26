namespace DeclarationEmployer.Contracts.Declarations.AnnexA1;

public sealed class AnnexA1LineValidationDto
{
    public Guid LineId { get; set; }

    public bool IsValid { get; set; }

    public IReadOnlyList<string> BlockingIssues { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}
