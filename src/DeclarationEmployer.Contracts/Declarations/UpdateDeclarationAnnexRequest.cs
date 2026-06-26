namespace DeclarationEmployer.Contracts.Declarations;

public sealed class UpdateDeclarationAnnexRequest
{
    public string AnnexCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
