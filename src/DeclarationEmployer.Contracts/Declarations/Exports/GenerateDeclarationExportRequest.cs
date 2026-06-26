namespace DeclarationEmployer.Contracts.Declarations;

public sealed class GenerateDeclarationExportRequest
{
    public string? Format { get; set; } = "CSV";

    public string? Notes { get; set; }
}
