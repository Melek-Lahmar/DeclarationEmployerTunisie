using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IGeneratedFilesService
{
    Task<IReadOnlyList<GeneratedFileDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default);
}
