namespace DeclarationEmployer.Contracts.Declarations;

public sealed class UpdateDeclarationBeneficiaryRequest
{
    public string IdentifierType { get; set; } = string.Empty;

    public string Identifier { get; set; } = string.Empty;

    public string FullNameOrCompanyName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Country { get; set; }

    public bool IsResident { get; set; } = true;
}
