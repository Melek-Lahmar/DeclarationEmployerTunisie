using DeclarationEmployer.Contracts.Archive;

namespace DeclarationEmployer.Application.Declarations;

public interface IArchiveService
{
    Task<ArchiveDeclarationResultDto> ArchiveAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);
}
