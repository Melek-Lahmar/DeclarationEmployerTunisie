using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationAnomaliesService
{
    Task<IReadOnlyList<DeclarationAnomalyDto>> GetByDeclarationAsync(Guid declarationId, string? severity, bool includeResolved, CancellationToken cancellationToken = default);

    Task<DeclarationAnomalyDto> ResolveAsync(Guid declarationId, Guid anomalyId, ResolveDeclarationAnomalyRequest request, CancellationToken cancellationToken = default);
}
