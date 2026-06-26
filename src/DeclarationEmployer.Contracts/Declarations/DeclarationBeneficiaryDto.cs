namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationBeneficiaryDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public string IdentifierType { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;

    public string FullNameOrCompanyName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Country { get; set; }

    public bool IsResident { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
