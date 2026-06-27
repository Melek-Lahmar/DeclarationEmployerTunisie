namespace DeclarationEmployer.Domain.Declarations;

using DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class DeclarationLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public Guid? AnnexId { get; set; }

    public Guid? BeneficiaryId { get; set; }

    public int? OrderNumber { get; set; }

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

    public AnnexA1Detail? AnnexA1Detail { get; set; }

    public AnnexA2Detail? AnnexA2Detail { get; set; }

    public AnnexA3Detail? AnnexA3Detail { get; set; }

    public AnnexA4Detail? AnnexA4Detail { get; set; }

    public AnnexA5Detail? AnnexA5Detail { get; set; }

    public AnnexA6Detail? AnnexA6Detail { get; set; }

    public AnnexA7Detail? AnnexA7Detail { get; set; }
}
