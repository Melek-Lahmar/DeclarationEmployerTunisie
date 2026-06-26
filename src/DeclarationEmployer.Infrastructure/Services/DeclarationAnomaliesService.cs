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

public sealed class DeclarationAnomaliesService : DeclarationServiceBase, IDeclarationAnomaliesService
{
    private readonly IValidator<ResolveDeclarationAnomalyRequest> _resolveValidator;

    public DeclarationAnomaliesService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<ResolveDeclarationAnomalyRequest> resolveValidator)
        : base(db, currentUserService, environment)
    {
        _resolveValidator = resolveValidator;
    }

    public async Task<IReadOnlyList<DeclarationAnomalyDto>> GetByDeclarationAsync(Guid declarationId, string? severity, bool includeResolved, CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        var query = Db.DeclarationAnomalies
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId);

        if (!includeResolved)
        {
            query = query.Where(x => !x.IsResolved);
        }

        if (!string.IsNullOrWhiteSpace(severity) &&
            Enum.TryParse<DeclarationAnomalySeverity>(severity, true, out var parsedSeverity))
        {
            query = query.Where(x => x.Severity == parsedSeverity);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeclarationAnomalyDto> ResolveAsync(Guid declarationId, Guid anomalyId, ResolveDeclarationAnomalyRequest request, CancellationToken cancellationToken = default)
    {
        await _resolveValidator.ValidateAndThrowAsync(request, cancellationToken);
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var anomaly = await Db.DeclarationAnomalies.FirstOrDefaultAsync(x => x.Id == anomalyId && x.DeclarationId == declarationId, cancellationToken);
        if (anomaly is null)
        {
            throw new ApplicationNotFoundException("Anomalie introuvable.");
        }

        anomaly.IsResolved = true;
        anomaly.ResolvedAt = DateTimeOffset.UtcNow;
        anomaly.ResolvedBy = GetAuditUserName();

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? string.Empty : $" Motif : {request.Reason.Trim()}";
        AddAudit("ANOMALY_RESOLVED", nameof(DeclarationAnomaly), anomaly.Id.ToString(), $"Resolution anomalie : {anomaly.Code}.{reason}");
        AddEvent(declarationId, "ANOMALY_RESOLVED", $"Resolution anomalie {anomaly.Code}.{reason}");
        await Db.SaveChangesAsync(cancellationToken);

        return ToDto(anomaly);
    }

    private static DeclarationAnomalyDto ToDto(DeclarationAnomaly entity) => new()
    {
        Id = entity.Id,
        DeclarationId = entity.DeclarationId,
        Severity = entity.Severity.ToString(),
        Code = entity.Code,
        Message = entity.Message,
        EntityName = entity.EntityName,
        EntityId = entity.EntityId,
        IsResolved = entity.IsResolved,
        CreatedAt = entity.CreatedAt,
        ResolvedAt = entity.ResolvedAt,
        ResolvedBy = entity.ResolvedBy
    };
}
