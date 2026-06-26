namespace DeclarationEmployer.Import;

public sealed class ExcelImportOptions
{
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}
