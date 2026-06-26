namespace DeclarationEmployer.FiscalEngine;

public sealed class FiscalControlLine
{
    public Guid LineId { get; set; }

    public Guid? BeneficiaryId { get; set; }

    public string? BeneficiaryIdentifier { get; set; }

    public string? BeneficiaryName { get; set; }

    public string OperationType { get; set; } = string.Empty;

    public string? FiscalCategory { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal WithheldAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? DocumentReference { get; set; }
}
