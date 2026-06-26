using DeclarationEmployer.Contracts.Import;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationImportService
{
    Task<ExcelImportPreviewDto> PreviewAsync(
        Guid declarationId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<ExcelImportCommitResultDto> CommitAsync(
        Guid declarationId,
        ExcelImportCommitRequest request,
        CancellationToken cancellationToken = default);
}
