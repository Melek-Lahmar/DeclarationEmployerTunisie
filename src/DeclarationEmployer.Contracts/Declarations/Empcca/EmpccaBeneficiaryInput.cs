namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class EmpccaBeneficiaryInput
{
    public int IdentifierType { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Activity { get; set; }
    public string? JobTitle { get; set; }
    public string Address { get; set; } = string.Empty;
}
