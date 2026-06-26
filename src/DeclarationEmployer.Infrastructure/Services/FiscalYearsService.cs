using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Cabinet;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class FiscalYearsService : IFiscalYearsService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHostEnvironment _environment;
    private readonly IValidator<CreateFiscalYearRequest> _createValidator;
    private readonly IValidator<UpdateFiscalYearRequest> _updateValidator;

    public FiscalYearsService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<CreateFiscalYearRequest> createValidator,
        IValidator<UpdateFiscalYearRequest> updateValidator)
    {
        _db = db;
        _currentUserService = currentUserService;
        _environment = environment;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<FiscalYearDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.FiscalYears
            .AsNoTracking()
            .Include(x => x.ClientCompany)
            .OrderByDescending(x => x.Year)
            .ThenBy(x => x.ClientCompany!.Code)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<FiscalYearDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.FiscalYears
            .AsNoTracking()
            .Include(x => x.ClientCompany)
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FiscalYearDto>> GetByClientAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return await _db.FiscalYears
            .AsNoTracking()
            .Include(x => x.ClientCompany)
            .Where(x => x.ClientCompanyId == clientId)
            .OrderByDescending(x => x.Year)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<FiscalYearDto> CreateAsync(
        Guid clientId,
        CreateFiscalYearRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var clientExists = await _db.Clients
            .AnyAsync(x => x.Id == clientId, cancellationToken);

        if (!clientExists)
        {
            throw new ApplicationNotFoundException("Société cliente introuvable.");
        }

        var exists = await _db.FiscalYears
            .AnyAsync(x => x.ClientCompanyId == clientId && x.Year == request.Year, cancellationToken);

        if (exists)
        {
            throw new ApplicationConflictException("Cet exercice fiscal existe déjà pour cette société.");
        }

        var entity = new FiscalYear
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = clientId,
            Year = request.Year,
            IsClosed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.FiscalYears.Add(entity);
        AddAudit(
            "FISCAL_YEAR_CREATED",
            nameof(FiscalYear),
            entity.Id.ToString(),
            $"Création exercice fiscal {entity.Year} pour client {entity.ClientCompanyId}",
            ipAddress);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task<FiscalYearDto> UpdateAsync(
        Guid id,
        UpdateFiscalYearRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _db.FiscalYears
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Exercice fiscal introuvable.");
        }

        if (entity.IsClosed)
        {
            throw new ApplicationConflictException("Impossible de modifier un exercice fiscal cloture.");
        }

        var duplicate = await _db.FiscalYears
            .AnyAsync(x => x.ClientCompanyId == entity.ClientCompanyId &&
                           x.Year == request.Year &&
                           x.Id != id,
                cancellationToken);

        if (duplicate)
        {
            throw new ApplicationConflictException("Cet exercice fiscal existe déjà pour cette société.");
        }

        entity.Year = request.Year;
        AddAudit(
            "FISCAL_YEAR_UPDATED",
            nameof(FiscalYear),
            entity.Id.ToString(),
            $"Modification exercice fiscal {entity.Year} pour client {entity.ClientCompanyId}",
            ipAddress);

        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task<FiscalYearDto> CloseAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.FiscalYears
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Exercice fiscal introuvable.");
        }

        if (!entity.IsClosed)
        {
            entity.IsClosed = true;
            entity.ClosedAt = DateTimeOffset.UtcNow;
            AddAudit(
                "FISCAL_YEAR_CLOSED",
                nameof(FiscalYear),
                entity.Id.ToString(),
                $"Clôture exercice fiscal {entity.Year} pour client {entity.ClientCompanyId}",
                ipAddress);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task<FiscalYearDto> ReopenAsync(
        Guid id,
        ReopenFiscalYearRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ApplicationConflictException("La justification de reouverture est obligatoire.");
        }

        var entity = await _db.FiscalYears
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new ApplicationNotFoundException("Exercice fiscal introuvable.");
        }

        if (entity.IsClosed)
        {
            entity.IsClosed = false;
            entity.ClosedAt = null;
            AddAudit(
                "FISCAL_YEAR_REOPENED",
                nameof(FiscalYear),
                entity.Id.ToString(),
                $"Reouverture exercice fiscal {entity.Year} pour client {entity.ClientCompanyId}. Justification : {request.Reason.Trim()}",
                ipAddress);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? ToDto(entity);
    }

    public async Task<FiscalYearStatusDto?> GetStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.FiscalYears
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FiscalYearStatusDto
            {
                Id = x.Id,
                Year = x.Year,
                IsClosed = x.IsClosed,
                ClosedAt = x.ClosedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FiscalYearHistoryDto>> GetHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.AuditLogs
            .AsNoTracking()
            .Where(x => x.EntityName == nameof(FiscalYear) && x.EntityId == id.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new FiscalYearHistoryDto
            {
                Action = x.Action,
                Details = x.Details,
                UserName = x.UserName,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);
    }

    private static FiscalYearDto ToDto(FiscalYear entity)
    {
        return new FiscalYearDto
        {
            Id = entity.Id,
            ClientCompanyId = entity.ClientCompanyId,
            ClientCode = entity.ClientCompany?.Code,
            ClientRaisonSociale = entity.ClientCompany?.RaisonSociale,
            Year = entity.Year,
            IsClosed = entity.IsClosed,
            ClosedAt = entity.ClosedAt,
            CreatedAt = entity.CreatedAt
        };
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
            UserName = GetAuditUserName(),
            Details = details,
            IpAddress = ipAddress,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }

    private string GetAuditUserName()
    {
        if (_currentUserService.IsAuthenticated && !string.IsNullOrWhiteSpace(_currentUserService.UserName))
        {
            return _currentUserService.UserName!;
        }

        return _environment.IsDevelopment() ? "system-dev" : "system";
    }
}
