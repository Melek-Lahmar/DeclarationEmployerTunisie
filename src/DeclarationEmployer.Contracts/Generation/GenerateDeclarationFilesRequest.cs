namespace DeclarationEmployer.Contracts.Generation;

public sealed class GenerateDeclarationFilesRequest
{
    public bool OfficialModeRequested { get; set; }

    public string? Notes { get; set; }
}
