namespace DeclarationEmployer.Contracts.Declarations.AnnexA1;

public sealed class CreateAnnexA1LineRequest
{
    public string BeneficiaryIdentifierType { get; set; } = "Other";

    public string BeneficiaryIdentifier { get; set; } = string.Empty;

    public string BeneficiaryName { get; set; } = string.Empty;

    public string? BeneficiaryAddress { get; set; }

    public string OperationType { get; set; } = "A1";

    public string? FiscalCategory { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal WithheldAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Notes { get; set; }
}
