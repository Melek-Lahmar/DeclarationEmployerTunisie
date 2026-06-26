using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationExportService
{
    Task<DeclarationExportPreviewDto> PreviewAsync(Guid declarationId, CancellationToken cancellationToken = default);

    Task<DeclarationExportResultDto> GenerateAsync(
        Guid declarationId,
        GenerateDeclarationExportRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeneratedFileDto>> GetGeneratedFilesAsync(Guid declarationId, CancellationToken cancellationToken = default);
}
