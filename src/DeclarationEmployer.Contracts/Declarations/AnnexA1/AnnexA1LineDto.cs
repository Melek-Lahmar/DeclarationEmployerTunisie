namespace DeclarationEmployer.Contracts.Declarations.AnnexA1;

public sealed class AnnexA1LineDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public Guid AnnexId { get; set; }

    public Guid BeneficiaryId { get; set; }

    public string BeneficiaryIdentifierType { get; set; } = string.Empty;

    public string BeneficiaryIdentifier { get; set; } = string.Empty;

    public string BeneficiaryName { get; set; } = string.Empty;

    public string? BeneficiaryAddress { get; set; }

    public string OperationType { get; set; } = string.Empty;

    public string? FiscalCategory { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal WithheldAmount { get; set; }

    public decimal NetPaidAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = string.Empty;
}
