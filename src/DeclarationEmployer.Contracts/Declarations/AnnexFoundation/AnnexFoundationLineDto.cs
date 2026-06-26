namespace DeclarationEmployer.Contracts.Declarations.AnnexFoundation;

public sealed class AnnexFoundationLineDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public Guid AnnexId { get; set; }

    public Guid BeneficiaryId { get; set; }

    public string AnnexCode { get; set; } = string.Empty;

    public int? OrderNumber { get; set; }

    public string BeneficiaryIdentifier { get; set; } = string.Empty;

    public string BeneficiaryName { get; set; } = string.Empty;

    public string OperationType { get; set; } = string.Empty;

    public decimal GrossAmount { get; set; }

    public decimal WithholdingAmount { get; set; }

    public decimal NetAmount { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsOfficialMappingConfirmed { get; set; }
}
