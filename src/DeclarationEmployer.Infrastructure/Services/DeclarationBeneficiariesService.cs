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

public sealed class DeclarationBeneficiariesService : DeclarationServiceBase, IDeclarationBeneficiariesService
{
    private readonly IValidator<CreateDeclarationBeneficiaryRequest> _createValidator;
    private readonly IValidator<UpdateDeclarationBeneficiaryRequest> _updateValidator;

    public DeclarationBeneficiariesService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<CreateDeclarationBeneficiaryRequest> createValidator,
        IValidator<UpdateDeclarationBeneficiaryRequest> updateValidator)
        : base(db, currentUserService, environment)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<DeclarationBeneficiaryDto>> GetByDeclarationAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        return await Db.DeclarationBeneficiaries
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderBy(x => x.FullNameOrCompanyName)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeclarationBeneficiaryDto> CreateAsync(Guid declarationId, CreateDeclarationBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            IdentifierType = ParseIdentifierType(request.IdentifierType),
            Identifier = request.Identifier.Trim(),
            FullNameOrCompanyName = request.FullNameOrCompanyName.Trim(),
            Address = Normalize(request.Address),
            Country = Normalize(request.Country),
            IsResident = request.IsResident,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationBeneficiaries.Add(entity);
        AddAudit("BENEFICIARY_CREATED", nameof(DeclarationBeneficiary), entity.Id.ToString(), $"Creation beneficiaire : {entity.FullNameOrCompanyName}");
        AddEvent(declarationId, "BENEFICIARY_CREATED", $"Creation beneficiaire {entity.FullNameOrCompanyName}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<DeclarationBeneficiaryDto> UpdateAsync(Guid declarationId, Guid beneficiaryId, UpdateDeclarationBeneficiaryRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = await Db.DeclarationBeneficiaries.FirstOrDefaultAsync(x => x.Id == beneficiaryId && x.DeclarationId == declarationId, cancellationToken);
        if (entity is null)
        {
            throw new ApplicationNotFoundException("Beneficiaire introuvable.");
        }

        entity.IdentifierType = ParseIdentifierType(request.IdentifierType);
        entity.Identifier = request.Identifier.Trim();
        entity.FullNameOrCompanyName = request.FullNameOrCompanyName.Trim();
        entity.Address = Normalize(request.Address);
        entity.Country = Normalize(request.Country);
        entity.IsResident = request.IsResident;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AddAudit("BENEFICIARY_UPDATED", nameof(DeclarationBeneficiary), entity.Id.ToString(), $"Modification beneficiaire : {entity.FullNameOrCompanyName}");
        AddEvent(declarationId, "BENEFICIARY_UPDATED", $"Modification beneficiaire {entity.FullNameOrCompanyName}.");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task DeleteAsync(Guid declarationId, Guid beneficiaryId, CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var entity = await Db.DeclarationBeneficiaries.FirstOrDefaultAsync(x => x.Id == beneficiaryId && x.DeclarationId == declarationId, cancellationToken);
        if (entity is null)
        {
            throw new ApplicationNotFoundException("Beneficiaire introuvable.");
        }

        var isUsed = await Db.DeclarationLines.AnyAsync(x => x.BeneficiaryId == beneficiaryId, cancellationToken);
        if (isUsed)
        {
            throw new ApplicationConflictException("Impossible de supprimer ce beneficiaire car il est utilise par des lignes.");
        }

        Db.DeclarationBeneficiaries.Remove(entity);
        AddAudit("BENEFICIARY_DELETED", nameof(DeclarationBeneficiary), entity.Id.ToString(), $"Suppression beneficiaire : {entity.FullNameOrCompanyName}");
        AddEvent(declarationId, "BENEFICIARY_DELETED", $"Suppression beneficiaire {entity.FullNameOrCompanyName}.");
        await Db.SaveChangesAsync(cancellationToken);
    }

    private static BeneficiaryIdentifierType ParseIdentifierType(string type)
    {
        if (!Enum.TryParse<BeneficiaryIdentifierType>(type, true, out var parsed))
        {
            throw new ApplicationConflictException("Type d'identifiant beneficiaire invalide.");
        }

        return parsed;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DeclarationBeneficiaryDto ToDto(DeclarationBeneficiary entity) => new()
    {
        Id = entity.Id,
        DeclarationId = entity.DeclarationId,
        IdentifierType = entity.IdentifierType.ToString(),
        Identifier = entity.Identifier,
        FullNameOrCompanyName = entity.FullNameOrCompanyName,
        Address = entity.Address,
        Country = entity.Country,
        IsResident = entity.IsResident,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
