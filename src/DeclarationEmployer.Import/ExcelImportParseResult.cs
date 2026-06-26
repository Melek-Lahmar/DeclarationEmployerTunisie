namespace DeclarationEmployer.Import;

public sealed class ExcelImportParseResult
{
    public IReadOnlyList<ExcelImportParsedRow> Rows { get; init; } = [];

    public IReadOnlyList<ExcelImportValidationIssue> Issues { get; init; } = [];
}
