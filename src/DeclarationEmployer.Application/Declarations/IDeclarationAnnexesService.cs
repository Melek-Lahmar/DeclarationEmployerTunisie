using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationAnnexesService
{
    Task<IReadOnlyList<DeclarationAnnexDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default);

    Task<DeclarationAnnexDto> CreateAsync(Guid declarationId, CreateDeclarationAnnexRequest request, CancellationToken cancellationToken = default);

    Task<DeclarationAnnexDto> UpdateAsync(Guid declarationId, Guid annexId, UpdateDeclarationAnnexRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid declarationId, Guid annexId, CancellationToken cancellationToken = default);
}
