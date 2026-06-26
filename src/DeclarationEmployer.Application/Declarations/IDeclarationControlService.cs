using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationControlService
{
    Task<DeclarationControlResultDto> ControlAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeclarationAnomalyDto>> GetAnomaliesAsync(
        Guid declarationId,
        string? severity,
        bool includeResolved,
        CancellationToken cancellationToken = default);

    Task<DeclarationAnomalyDto> ResolveAnomalyAsync(
        Guid declarationId,
        Guid anomalyId,
        ResolveDeclarationAnomalyRequest request,
        CancellationToken cancellationToken = default);
}
