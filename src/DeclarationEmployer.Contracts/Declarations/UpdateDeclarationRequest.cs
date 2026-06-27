namespace DeclarationEmployer.Contracts.Declarations;

public sealed class UpdateDeclarationRequest
{
    public int ActCode { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
