namespace DeclarationEmployer.Contracts.Declarations.AnnexFoundation;

public sealed class AnnexFoundationSummaryDto
{
    public Guid DeclarationId { get; set; }

    public string AnnexCode { get; set; } = string.Empty;

    public int LinesCount { get; set; }

    public int BeneficiariesCount { get; set; }

    public decimal GrossAmountTotal { get; set; }

    public decimal WithholdingAmountTotal { get; set; }

    public decimal NetAmountTotal { get; set; }

    public bool IsOfficialMappingConfirmed { get; set; }

    public string MappingMessage { get; set; } = string.Empty;
}
