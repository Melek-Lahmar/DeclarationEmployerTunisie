namespace DeclarationEmployer.Contracts.Declarations.AnnexFoundation;

public sealed class CreateAnnexFoundationLineRequest
{
    public int? OrderNumber { get; set; }

    public string BeneficiaryIdentifier { get; set; } = string.Empty;

    public string BeneficiaryName { get; set; } = string.Empty;

    public string OperationType { get; set; } = string.Empty;

    public decimal GrossAmount { get; set; }

    public decimal WithholdingAmount { get; set; }

    public decimal NetAmount { get; set; }

    public string? Notes { get; set; }
}
