using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IArchivedDocumentsService
{
    Task<IReadOnlyList<ArchivedDocumentDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArchivedDocumentDto>> GetByClientAndYearAsync(Guid? clientId, int? year, CancellationToken cancellationToken = default);
}
