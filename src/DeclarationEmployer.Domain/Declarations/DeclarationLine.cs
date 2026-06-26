namespace DeclarationEmployer.Domain.Declarations;

public sealed class DeclarationLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

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

    public DeclarationLineStatus Status { get; set; } = DeclarationLineStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public EmployerDeclaration? Declaration { get; set; }

    public DeclarationAnnex? Annex { get; set; }

    public DeclarationBeneficiary? Beneficiary { get; set; }
}
