namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationLineDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public Guid? AnnexId { get; set; }

    public Guid? BeneficiaryId { get; set; }

    public string OperationType { get; set; } = string.Empty;

    public string? FiscalCategory { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal TaxableAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal WithheldAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? DocumentReference { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
