using DeclarationEmployer.Contracts.Declarations.AnnexFoundation;

namespace DeclarationEmployer.Application.Declarations;

public interface IAnnexFoundationService
{
    Task<IReadOnlyList<AnnexFoundationLineDto>> GetLinesAsync(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken = default);

    Task<AnnexFoundationLineDto> CreateLineAsync(
        Guid declarationId,
        string annexCode,
        CreateAnnexFoundationLineRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteLineAsync(
        Guid declarationId,
        string annexCode,
        Guid lineId,
        CancellationToken cancellationToken = default);

    Task<AnnexFoundationSummaryDto> GetSummaryAsync(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken = default);
}
