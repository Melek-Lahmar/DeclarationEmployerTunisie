namespace DeclarationEmployer.Contracts.Declarations;

public sealed class UpdateDeclarationRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
