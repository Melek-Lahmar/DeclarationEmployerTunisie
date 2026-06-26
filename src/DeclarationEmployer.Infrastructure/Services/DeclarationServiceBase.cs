using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public abstract class DeclarationServiceBase
{
    protected readonly ApplicationDbContext Db;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHostEnvironment _environment;

    protected DeclarationServiceBase(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment)
    {
        Db = db;
        _currentUserService = currentUserService;
        _environment = environment;
    }

    protected async Task<EmployerDeclaration> GetDeclarationAsync(Guid declarationId, CancellationToken cancellationToken)
    {
        var declaration = await Db.Declarations
            .FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken);

        return declaration ?? throw new ApplicationNotFoundException("Declaration introuvable.");
    }

    protected async Task<EmployerDeclaration> GetEditableDeclarationAsync(Guid declarationId, CancellationToken cancellationToken)
    {
        var declaration = await GetDeclarationAsync(declarationId, cancellationToken);

        if (declaration.IsLocked || declaration.Status is DeclarationStatus.Closed or DeclarationStatus.Archived)
        {
            throw new ApplicationConflictException("Impossible de modifier une declaration verrouillee, cloturee ou archivee.");
        }

        return declaration;
    }

    protected string GetAuditUserName()
    {
        if (_currentUserService.IsAuthenticated && !string.IsNullOrWhiteSpace(_currentUserService.UserName))
        {
            return _currentUserService.UserName!;
        }

        return _environment.IsDevelopment() ? "system-dev" : "system";
    }

    protected void AddAudit(string action, string entityName, string? entityId, string details)
    {
        Db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserName = GetAuditUserName(),
            Details = details,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }

    protected void AddEvent(Guid declarationId, string action, string description)
    {
        Db.DeclarationEvents.Add(new DeclarationEvent
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            Action = action,
            Description = description,
            UserName = GetAuditUserName(),
            OccurredAt = DateTimeOffset.UtcNow
        });
    }
}
