namespace DeclarationEmployer.Contracts.Declarations.AnnexA1;

public sealed class AnnexA1SummaryDto
{
    public Guid DeclarationId { get; set; }

    public int LinesCount { get; set; }

    public int BeneficiariesCount { get; set; }

    public decimal GrossAmountTotal { get; set; }

    public decimal TaxableAmountTotal { get; set; }

    public decimal WithheldAmountTotal { get; set; }

    public decimal NetPaidAmountTotal { get; set; }

    public bool IsOfficialMappingConfirmed { get; set; }

    public string MappingMessage { get; set; } = string.Empty;
}
