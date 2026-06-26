namespace DeclarationEmployer.Contracts.Declarations;

public sealed class CreateDeclarationAnnexRequest
{
    public string AnnexCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
