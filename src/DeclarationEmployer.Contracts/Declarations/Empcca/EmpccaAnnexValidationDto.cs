namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class EmpccaAnnexValidationDto
{
    public bool IsValid => BlockingIssues.Count == 0;
    public IReadOnlyList<string> BlockingIssues { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}

public sealed class EmpccaAnnexSummaryDto
{
    public string AnnexCode { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public decimal GrossAmountTotal { get; set; }
    public decimal WithheldAmountTotal { get; set; }
    public decimal NetPaidAmountTotal { get; set; }
}
