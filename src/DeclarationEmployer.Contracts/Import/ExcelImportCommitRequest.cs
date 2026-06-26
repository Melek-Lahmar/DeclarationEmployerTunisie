namespace DeclarationEmployer.Contracts.Import;

public sealed class ExcelImportCommitRequest
{
    public string TemporaryFileToken { get; set; } = string.Empty;

    public bool ImportOnlyValidRows { get; set; } = true;
}
