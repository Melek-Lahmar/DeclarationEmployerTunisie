namespace DeclarationEmployer.Domain.Declarations;

public sealed class DeclarationBeneficiary
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public BeneficiaryIdentifierType IdentifierType { get; set; } = BeneficiaryIdentifierType.Other;

    public string Identifier { get; set; } = string.Empty;

    public string FullNameOrCompanyName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Country { get; set; }

    public bool IsResident { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public EmployerDeclaration? Declaration { get; set; }
}
