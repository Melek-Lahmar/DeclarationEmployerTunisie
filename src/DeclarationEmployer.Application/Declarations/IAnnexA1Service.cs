using DeclarationEmployer.Contracts.Declarations.AnnexA1;

namespace DeclarationEmployer.Application.Declarations;

public interface IAnnexA1Service
{
    Task<IReadOnlyList<AnnexA1LineDto>> GetLinesAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<AnnexA1LineDto> CreateLineAsync(
        Guid declarationId,
        CreateAnnexA1LineRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteLineAsync(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default);

    Task<AnnexA1SummaryDto> GetSummaryAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<AnnexA1LineValidationDto> ValidateLineAsync(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default);
}
