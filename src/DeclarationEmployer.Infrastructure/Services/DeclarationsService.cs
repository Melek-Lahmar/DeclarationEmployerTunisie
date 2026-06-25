using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DeclarationsService : IDeclarationsService
{
    private readonly ApplicationDbContext _db;
    private readonly IValidator<CreateDeclarationRequest> _createValidator;
    private readonly IValidator<UpdateDeclarationRequest> _updateValidator;

    public DeclarationsService(
        ApplicationDbContext db,
        IValidator<CreateDeclarationRequest> createValidator,
        IValidator<UpdateDeclarationRequest> updateValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<DeclarationDto>> GetAllAsync(
        Guid? clientId,
        Guid? fiscalYearId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Declarations
            .AsNoTracking()
            .Include(x => x.ClientCompany)
            .Include(x => x.FiscalYear)
            .AsQueryable();

        if (clientId.HasValue)
        {
            query = query.Where(x => x.ClientCompanyId == clientId.Value);
        }

        if (fiscalYearId.HasValue)
        {
            query = query.Where(x => x.FiscalYearId == fiscalYearId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<DeclarationStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        return await query
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.ClientCompany!.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeclarationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Declarations
            .AsNoTracking()
            .Include(x => x.ClientCompany)
            .Include(x => x.FiscalYear)
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DeclarationDto> CreateAsync(
        CreateDeclarationRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var fiscalYear = await _db.FiscalYears
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
        {
            throw new ApplicationNotFoundException("Exercice fiscal introuvable.");
        }

        if (fiscalYear.ClientCompanyId != request.ClientCompanyId)
        {
            throw new ApplicationConflictException("L'exercice fiscal ne correspond pas a cette societe.");
        }

        var exists = await _db.Declarations
            .AnyAsync(x => x.ClientCompanyId == request.ClientCompanyId &&
                           x.FiscalYearId == request.FiscalYearId,
                cancellationToken);

        if (exists)
        {
            throw new ApplicationConflictException("Une declaration existe deja pour cette societe et cet exercice.");
        }

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"Declaration employeur {fiscalYear.Year}"
            : request.Title.Trim();

        var entity = new EmployerDeclaration
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = request.ClientCompanyId,
            FiscalYearId = request.FiscalYearId,
            Year = fiscalYear.Year,
            Status = DeclarationStatus.Draft,
            Title = title,
            Notes = Normalize(request.Notes),
            CreatedBy = "system-dev",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Declarations.Add(entity);
        AddEvent(entity.Id, "DECLARATION_CREATED", $"Creation declaration {entity.Year}.");
        AddAudit("DECLARATION_CREATED", entity.Id, $"Creation declaration : {entity.Title}", ipAddress);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task<DeclarationDto> UpdateAsync(
        Guid id,
        UpdateDeclarationRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _db.Declarations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Declaration introuvable.");
        }

        EnsureEditable(entity);

        entity.Title = request.Title.Trim();
        entity.Notes = Normalize(request.Notes);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AddEvent(entity.Id, "DECLARATION_UPDATED", $"Modification declaration {entity.Year}.");
        AddAudit("DECLARATION_UPDATED", entity.Id, $"Modification declaration : {entity.Title}", ipAddress);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task DeleteAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Declarations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Declaration introuvable.");
        }

        EnsureEditable(entity);

        entity.IsLocked = true;
        entity.LockedAt = DateTimeOffset.UtcNow;
        entity.LockedBy = "system-dev";
        entity.Status = DeclarationStatus.Closed;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        AddEvent(entity.Id, "DECLARATION_DELETED", "Suppression logique de la declaration.");
        AddAudit("DECLARATION_DELETED", entity.Id, $"Suppression logique declaration : {entity.Title}", ipAddress);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeclarationSummaryDto?> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaration = await GetByIdAsync(id, cancellationToken);

        if (declaration is null)
        {
            return null;
        }

        var lastEvent = await _db.DeclarationEvents
            .AsNoTracking()
            .Where(x => x.DeclarationId == id)
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new DeclarationSummaryDto
        {
            Declaration = declaration,
            AnnexesCount = 0,
            BlockingAnomaliesCount = 0,
            GeneratedFilesCount = 0,
            LastEventAction = lastEvent?.Action,
            LastEventAt = lastEvent?.OccurredAt
        };
    }

    public async Task<IReadOnlyList<DeclarationEventDto>> GetEventsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.DeclarationEvents
            .AsNoTracking()
            .Where(x => x.DeclarationId == id)
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new DeclarationEventDto
            {
                Id = x.Id,
                DeclarationId = x.DeclarationId,
                Action = x.Action,
                Description = x.Description,
                UserName = x.UserName,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DeclarationDto> LockAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Declarations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Declaration introuvable.");
        }

        if (!entity.IsLocked)
        {
            entity.IsLocked = true;
            entity.LockedAt = DateTimeOffset.UtcNow;
            entity.LockedBy = "system-dev";
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            AddEvent(entity.Id, "DECLARATION_LOCKED", "Verrouillage declaration.");
            AddAudit("DECLARATION_LOCKED", entity.Id, $"Verrouillage declaration : {entity.Title}", ipAddress);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task<DeclarationDto> CloseAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Declarations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Declaration introuvable.");
        }

        if (entity.Status != DeclarationStatus.Closed)
        {
            entity.Status = DeclarationStatus.Closed;
            entity.IsLocked = true;
            entity.LockedAt ??= DateTimeOffset.UtcNow;
            entity.LockedBy ??= "system-dev";
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            AddEvent(entity.Id, "DECLARATION_CLOSED", "Cloture declaration.");
            AddAudit("DECLARATION_CLOSED", entity.Id, $"Cloture declaration : {entity.Title}", ipAddress);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    private static DeclarationDto ToDto(EmployerDeclaration entity)
    {
        return new DeclarationDto
        {
            Id = entity.Id,
            ClientCompanyId = entity.ClientCompanyId,
            ClientCode = entity.ClientCompany?.Code,
            ClientRaisonSociale = entity.ClientCompany?.RaisonSociale,
            FiscalYearId = entity.FiscalYearId,
            Year = entity.Year,
            Status = entity.Status.ToString(),
            Title = entity.Title,
            Notes = entity.Notes,
            IsLocked = entity.IsLocked,
            LockedAt = entity.LockedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static void EnsureEditable(EmployerDeclaration entity)
    {
        if (entity.IsLocked || entity.Status == DeclarationStatus.Closed)
        {
            throw new ApplicationConflictException("Impossible de modifier une declaration verrouillee ou cloturee.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private void AddEvent(Guid declarationId, string action, string description)
    {
        _db.DeclarationEvents.Add(new DeclarationEvent
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            Action = action,
            Description = description,
            UserName = "system-dev",
            OccurredAt = DateTimeOffset.UtcNow
        });
    }

    private void AddAudit(string action, Guid declarationId, string details, string? ipAddress)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityName = nameof(EmployerDeclaration),
            EntityId = declarationId.ToString(),
            UserName = "system-dev",
            Details = details,
            IpAddress = ipAddress,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }
}
