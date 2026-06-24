using DeclarationEmployer.Application.Cabinet;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Contracts.Common;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class ClientsService : IClientsService
{
    private readonly ApplicationDbContext _db;
    private readonly IValidator<CreateClientCompanyRequest> _createValidator;
    private readonly IValidator<UpdateClientCompanyRequest> _updateValidator;

    public ClientsService(
        ApplicationDbContext db,
        IValidator<CreateClientCompanyRequest> createValidator,
        IValidator<UpdateClientCompanyRequest> updateValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ClientCompanyDto>> GetAllAsync(
        bool includeInactive,
        string? search,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery(includeInactive, search)
            .OrderBy(x => x.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ClientCompanyDto>> GetPagedAsync(
        bool includeInactive,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = BuildQuery(includeInactive, search).OrderBy(x => x.Code);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientCompanyDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ClientCompanyDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Clients
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClientSummaryDto?> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var client = await GetByIdAsync(id, cancellationToken);

        if (client is null)
        {
            return null;
        }

        var fiscalYears = await _db.FiscalYears
            .AsNoTracking()
            .Where(x => x.ClientCompanyId == id)
            .ToListAsync(cancellationToken);

        var lastAudit = await _db.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityName == nameof(ClientCompany) && x.EntityId == id.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new ClientSummaryDto
        {
            Client = client,
            FiscalYearsCount = fiscalYears.Count,
            LastFiscalYear = fiscalYears.Count == 0 ? null : fiscalYears.Max(x => x.Year),
            DeclarationsCount = 0,
            LastAuditAction = lastAudit?.Action,
            LastAuditAt = lastAudit?.OccurredAt
        };
    }

    public async Task<ClientCompanyDto> CreateAsync(
        CreateClientCompanyRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var code = request.Code.Trim().ToUpperInvariant();
        var exists = await _db.Clients
            .AnyAsync(x => x.Code == code, cancellationToken);

        if (exists)
        {
            throw new ApplicationConflictException("Une société avec ce code existe déjà.");
        }

        var entity = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = code,
            RaisonSociale = request.RaisonSociale.Trim(),
            MatriculeFiscal = Normalize(request.MatriculeFiscal),
            Cle = Normalize(request.Cle),
            Categorie = Normalize(request.Categorie),
            CodeTva = Normalize(request.CodeTva),
            Etablissement = Normalize(request.Etablissement),
            Activite = Normalize(request.Activite),
            Adresse = Normalize(request.Adresse),
            Ville = Normalize(request.Ville),
            CodePostal = Normalize(request.CodePostal),
            Telephone = Normalize(request.Telephone),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Clients.Add(entity);
        AddAudit(
            "CLIENT_CREATED",
            nameof(ClientCompany),
            entity.Id.ToString(),
            $"Création société cliente : {entity.Code} - {entity.RaisonSociale}",
            ipAddress);

        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<ClientCompanyDto> UpdateAsync(
        Guid id,
        UpdateClientCompanyRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _db.Clients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Société cliente introuvable.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var duplicateCode = await _db.Clients
            .AnyAsync(x => x.Code == code && x.Id != id, cancellationToken);

        if (duplicateCode)
        {
            throw new ApplicationConflictException("Une autre société avec ce code existe déjà.");
        }

        entity.Code = code;
        entity.RaisonSociale = request.RaisonSociale.Trim();
        entity.MatriculeFiscal = Normalize(request.MatriculeFiscal);
        entity.Cle = Normalize(request.Cle);
        entity.Categorie = Normalize(request.Categorie);
        entity.CodeTva = Normalize(request.CodeTva);
        entity.Etablissement = Normalize(request.Etablissement);
        entity.Activite = Normalize(request.Activite);
        entity.Adresse = Normalize(request.Adresse);
        entity.Ville = Normalize(request.Ville);
        entity.CodePostal = Normalize(request.CodePostal);
        entity.Telephone = Normalize(request.Telephone);
        entity.IsActive = request.IsActive;

        AddAudit(
            "CLIENT_UPDATED",
            nameof(ClientCompany),
            entity.Id.ToString(),
            $"Modification société cliente : {entity.Code} - {entity.RaisonSociale}",
            ipAddress);

        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task DeleteAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Clients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Société cliente introuvable.");
        }

        entity.IsActive = false;
        AddAudit(
            "CLIENT_DEACTIVATED",
            nameof(ClientCompany),
            entity.Id.ToString(),
            $"Désactivation société cliente : {entity.Code} - {entity.RaisonSociale}",
            ipAddress);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ClientCompany> BuildQuery(bool includeInactive, string? search)
    {
        IQueryable<ClientCompany> query = _db.Clients.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Code.ToLower().Contains(term) ||
                x.RaisonSociale.ToLower().Contains(term) ||
                (x.MatriculeFiscal != null && x.MatriculeFiscal.ToLower().Contains(term)));
        }

        return query;
    }

    private static ClientCompanyDto ToDto(ClientCompany entity)
    {
        return new ClientCompanyDto
        {
            Id = entity.Id,
            Code = entity.Code,
            RaisonSociale = entity.RaisonSociale,
            MatriculeFiscal = entity.MatriculeFiscal,
            Cle = entity.Cle,
            Categorie = entity.Categorie,
            CodeTva = entity.CodeTva,
            Etablissement = entity.Etablissement,
            Activite = entity.Activite,
            Adresse = entity.Adresse,
            Ville = entity.Ville,
            CodePostal = entity.CodePostal,
            Telephone = entity.Telephone,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private void AddAudit(
        string action,
        string entityName,
        string? entityId,
        string? details,
        string? ipAddress)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserName = "system-dev",
            Details = details,
            IpAddress = ipAddress,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }
}
