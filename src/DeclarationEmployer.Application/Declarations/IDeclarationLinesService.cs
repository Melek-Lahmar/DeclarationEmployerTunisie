using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationLinesService
{
    Task<IReadOnlyList<DeclarationLineDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default);

    Task<DeclarationLineDto> CreateAsync(Guid declarationId, CreateDeclarationLineRequest request, CancellationToken cancellationToken = default);

    Task<DeclarationLineDto> UpdateAsync(Guid declarationId, Guid lineId, UpdateDeclarationLineRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default);
}
