namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class EmpccaAnnexValidationDto
{
    public bool IsValid => BlockingIssues.Count == 0;
    public IReadOnlyList<string> BlockingIssues { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}
