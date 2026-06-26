namespace DeclarationEmployer.Import;

public sealed class ExcelImportValidationIssue
{
    public int RowNumber { get; set; }

    public string? ColumnName { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
