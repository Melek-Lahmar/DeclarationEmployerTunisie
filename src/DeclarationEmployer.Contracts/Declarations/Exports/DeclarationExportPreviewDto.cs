namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationExportPreviewDto
{
    public Guid DeclarationId { get; set; }

    public int LinesCount { get; set; }

    public int BlockingAnomaliesCount { get; set; }

    public int WarningAnomaliesCount { get; set; }

    public int InfoAnomaliesCount { get; set; }

    public decimal TotalGrossAmount { get; set; }

    public decimal TotalTaxableAmount { get; set; }

    public decimal TotalWithheldAmount { get; set; }

    public bool CanGenerate { get; set; }

    public IReadOnlyList<string> BlockingMessages { get; set; } = [];
}
