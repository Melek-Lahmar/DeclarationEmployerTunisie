using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class GeneratedFilesService : IGeneratedFilesService
{
    private readonly ApplicationDbContext _db;

    public GeneratedFilesService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<GeneratedFileDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        return await _db.GeneratedFiles
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new GeneratedFileDto
            {
                Id = x.Id,
                DeclarationId = x.DeclarationId,
                FileType = x.FileType.ToString(),
                FileName = x.FileName,
                RelativePath = x.RelativePath,
                Sha256Hash = x.Sha256Hash,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy
            })
            .ToListAsync(cancellationToken);
    }
}
