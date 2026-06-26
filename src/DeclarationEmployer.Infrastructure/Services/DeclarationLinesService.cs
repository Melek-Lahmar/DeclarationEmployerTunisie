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

public sealed class DeclarationLinesService : DeclarationServiceBase, IDeclarationLinesService
{
    private readonly IValidator<CreateDeclarationLineRequest> _createValidator;
    private readonly IValidator<UpdateDeclarationLineRequest> _updateValidator;

    public DeclarationLinesService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<CreateDeclarationLineRequest> createValidator,
        IValidator<UpdateDeclarationLineRequest> updateValidator)
        : base(db, currentUserService, environment)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<DeclarationLineDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        return await Db.DeclarationLines
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeclarationLineDto> CreateAsync(Guid declarationId, CreateDeclarationLineRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        await ValidateReferencesAsync(declarationId, request.AnnexId, request.BeneficiaryId, cancellationToken);

        var entity = new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            AnnexId = request.AnnexId,
            BeneficiaryId = request.BeneficiaryId,
            OperationType = request.OperationType.Trim(),
            FiscalCategory = Normalize(request.FiscalCategory),
            GrossAmount = request.GrossAmount,
            TaxableAmount = request.TaxableAmount,
            Rate = request.Rate,
            WithheldAmount = request.WithheldAmount,
            PaymentDate = request.PaymentDate,
            DocumentReference = Normalize(request.DocumentReference),
            Notes = Normalize(request.Notes),
            Status = DeclarationLineStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationLines.Add(entity);
        AddAudit("LINE_CREATED", nameof(DeclarationLine), entity.Id.ToString(), $"Creation ligne declaration : {entity.OperationType}");
        AddEvent(declarationId, "LINE_CREATED", $"Creation ligne {entity.OperationType}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<DeclarationLineDto> UpdateAsync(Guid declarationId, Guid lineId, UpdateDeclarationLineRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);
        await ValidateReferencesAsync(declarationId, request.AnnexId, request.BeneficiaryId, cancellationToken);

        var entity = await Db.DeclarationLines.FirstOrDefaultAsync(x => x.Id == lineId && x.DeclarationId == declarationId, cancellationToken);
        if (entity is null)
        {
            throw new ApplicationNotFoundException("Ligne de declaration introuvable.");
        }

        entity.AnnexId = request.AnnexId;
        entity.BeneficiaryId = request.BeneficiaryId;
        entity.OperationType = request.OperationType.Trim();
        entity.FiscalCategory = Normalize(request.FiscalCategory);
        entity.GrossAmount = request.GrossAmount;
        entity.TaxableAmount = request.TaxableAmount;
        entity.Rate = request.Rate;
        entity.WithheldAmount = request.WithheldAmount;
        entity.PaymentDate = request.PaymentDate;
        entity.DocumentReference = Normalize(request.DocumentReference);
        entity.Notes = Normalize(request.Notes);
        entity.Status = ParseLineStatus(request.Status);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AddAudit("LINE_UPDATED", nameof(DeclarationLine), entity.Id.ToString(), $"Modification ligne declaration : {entity.OperationType}");
        AddEvent(declarationId, "LINE_UPDATED", $"Modification ligne {entity.OperationType}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task DeleteAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = await Db.DeclarationLines.FirstOrDefaultAsync(x => x.Id == lineId && x.DeclarationId == declarationId, cancellationToken);
        if (entity is null)
        {
            throw new ApplicationNotFoundException("Ligne de declaration introuvable.");
        }

        Db.DeclarationLines.Remove(entity);
        AddAudit("LINE_DELETED", nameof(DeclarationLine), entity.Id.ToString(), $"Suppression ligne declaration : {entity.OperationType}");
        AddEvent(declarationId, "LINE_DELETED", $"Suppression ligne {entity.OperationType}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateReferencesAsync(Guid declarationId, Guid? annexId, Guid? beneficiaryId, CancellationToken cancellationToken)
    {
        if (annexId.HasValue)
        {
            var annexExists = await Db.DeclarationAnnexes.AnyAsync(x => x.Id == annexId.Value && x.DeclarationId == declarationId, cancellationToken);
            if (!annexExists)
            {
                throw new ApplicationConflictException("Annexe de declaration invalide.");
            }
        }

        if (beneficiaryId.HasValue)
        {
            var beneficiaryExists = await Db.DeclarationBeneficiaries.AnyAsync(x => x.Id == beneficiaryId.Value && x.DeclarationId == declarationId, cancellationToken);
            if (!beneficiaryExists)
            {
                throw new ApplicationConflictException("Beneficiaire de declaration invalide.");
            }
        }
    }

    private static DeclarationLineStatus ParseLineStatus(string status)
    {
        if (!Enum.TryParse<DeclarationLineStatus>(status, true, out var parsed))
        {
            throw new ApplicationConflictException("Statut de ligne invalide.");
        }

        return parsed;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DeclarationLineDto ToDto(DeclarationLine entity) => new()
    {
        Id = entity.Id,
        DeclarationId = entity.DeclarationId,
        AnnexId = entity.AnnexId,
        BeneficiaryId = entity.BeneficiaryId,
        OperationType = entity.OperationType,
        FiscalCategory = entity.FiscalCategory,
        GrossAmount = entity.GrossAmount,
        TaxableAmount = entity.TaxableAmount,
        Rate = entity.Rate,
        WithheldAmount = entity.WithheldAmount,
        PaymentDate = entity.PaymentDate,
        DocumentReference = entity.DocumentReference,
        Notes = entity.Notes,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
