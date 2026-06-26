using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class ArchivedDocumentsService : IArchivedDocumentsService
{
    private readonly ApplicationDbContext _db;

    public ArchivedDocumentsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ArchivedDocumentDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        return await _db.ArchivedDocuments
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderByDescending(x => x.ArchivedAt)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ArchivedDocumentDto>> GetByClientAndYearAsync(Guid? clientId, int? year, CancellationToken cancellationToken = default)
    {
        var query = _db.ArchivedDocuments
            .AsNoTracking()
            .Include(x => x.FiscalYear)
            .AsQueryable();

        if (clientId.HasValue)
        {
            query = query.Where(x => x.ClientCompanyId == clientId.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(x => x.FiscalYear != null && x.FiscalYear.Year == year.Value);
        }

        return await query
            .OrderByDescending(x => x.ArchivedAt)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    private static ArchivedDocumentDto ToDto(Domain.Declarations.ArchivedDocument entity) => new()
    {
        Id = entity.Id,
        DeclarationId = entity.DeclarationId,
        ClientCompanyId = entity.ClientCompanyId,
        FiscalYearId = entity.FiscalYearId,
        DocumentType = entity.DocumentType.ToString(),
        FileName = entity.FileName,
        RelativePath = entity.RelativePath,
        Sha256Hash = entity.Sha256Hash,
        ArchivedAt = entity.ArchivedAt,
        ArchivedBy = entity.ArchivedBy
    };
}
