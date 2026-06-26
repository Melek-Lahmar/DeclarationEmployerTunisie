namespace DeclarationEmployer.Import;

public interface IExcelDeclarationImportService
{
    Task<ExcelImportParseResult> ParseAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
