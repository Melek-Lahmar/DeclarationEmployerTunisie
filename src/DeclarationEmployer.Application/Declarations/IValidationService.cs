using DeclarationEmployer.Contracts.Validation;

namespace DeclarationEmployer.Application.Declarations;

public interface IValidationService
{
    Task<ValidationRunSummaryDto> RunAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ValidationResultDto>> GetResultsAsync(
        Guid declarationId,
        bool includeResolved = false,
        CancellationToken cancellationToken = default);

    Task<ValidationResultDto> MarkCorrectedAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ValidationResultDto> IgnoreAsync(
        Guid id,
        IgnoreValidationResultRequest request,
        CancellationToken cancellationToken = default);
}
