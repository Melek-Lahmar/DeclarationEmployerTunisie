using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DeclarationAnnexesService : DeclarationServiceBase, IDeclarationAnnexesService
{
    private readonly IValidator<CreateDeclarationAnnexRequest> _createValidator;
    private readonly IValidator<UpdateDeclarationAnnexRequest> _updateValidator;

    public DeclarationAnnexesService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<CreateDeclarationAnnexRequest> createValidator,
        IValidator<UpdateDeclarationAnnexRequest> updateValidator)
        : base(db, currentUserService, environment)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<DeclarationAnnexDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        return await Db.DeclarationAnnexes
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderBy(x => x.AnnexCode)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeclarationAnnexDto> CreateAsync(Guid declarationId, CreateDeclarationAnnexRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = new DeclarationAnnex
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            AnnexCode = request.AnnexCode.Trim(),
            Title = request.Title.Trim(),
            Status = DeclarationAnnexStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationAnnexes.Add(entity);
        AddAudit("ANNEX_CREATED", nameof(DeclarationAnnex), entity.Id.ToString(), $"Creation annexe : {entity.AnnexCode} - {entity.Title}");
        AddEvent(declarationId, "ANNEX_CREATED", $"Creation annexe {entity.AnnexCode}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<DeclarationAnnexDto> UpdateAsync(Guid declarationId, Guid annexId, UpdateDeclarationAnnexRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = await Db.DeclarationAnnexes.FirstOrDefaultAsync(x => x.Id == annexId && x.DeclarationId == declarationId, cancellationToken);
        if (entity is null)
        {
            throw new ApplicationNotFoundException("Annexe introuvable.");
        }

        entity.AnnexCode = request.AnnexCode.Trim();
        entity.Title = request.Title.Trim();
        entity.Status = ParseAnnexStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AddAudit("ANNEX_UPDATED", nameof(DeclarationAnnex), entity.Id.ToString(), $"Modification annexe : {entity.AnnexCode} - {entity.Title}");
        AddEvent(declarationId, "ANNEX_UPDATED", $"Modification annexe {entity.AnnexCode}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task DeleteAsync(Guid declarationId, Guid annexId, CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = await Db.DeclarationAnnexes.FirstOrDefaultAsync(x => x.Id == annexId && x.DeclarationId == declarationId, cancellationToken);
        if (entity is null)
        {
            throw new ApplicationNotFoundException("Annexe introuvable.");
        }

        var isUsed = await Db.DeclarationLines.AnyAsync(x => x.AnnexId == annexId, cancellationToken);
        if (isUsed)
        {
            throw new ApplicationConflictException("Impossible de supprimer cette annexe car elle est utilisee par des lignes.");
        }

        Db.DeclarationAnnexes.Remove(entity);
        AddAudit("ANNEX_DELETED", nameof(DeclarationAnnex), entity.Id.ToString(), $"Suppression annexe : {entity.AnnexCode} - {entity.Title}");
        AddEvent(declarationId, "ANNEX_DELETED", $"Suppression annexe {entity.AnnexCode}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    private static DeclarationAnnexStatus ParseAnnexStatus(string status)
    {
        if (!Enum.TryParse<DeclarationAnnexStatus>(status, true, out var parsed))
        {
            throw new ApplicationConflictException("Statut d'annexe invalide.");
        }

        return parsed;
    }

    private static DeclarationAnnexDto ToDto(DeclarationAnnex entity) => new()
    {
        Id = entity.Id,
        DeclarationId = entity.DeclarationId,
        AnnexCode = entity.AnnexCode,
        Title = entity.Title,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
