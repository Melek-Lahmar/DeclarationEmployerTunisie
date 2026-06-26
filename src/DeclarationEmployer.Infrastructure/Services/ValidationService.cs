using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Validation;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class ValidationService : DeclarationServiceBase, IValidationService
{
    private readonly IDeclarationControlService _declarationControlService;

    public ValidationService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IDeclarationControlService declarationControlService)
        : base(db, currentUserService, environment)
    {
        _declarationControlService = declarationControlService;
    }

    public async Task<ValidationRunSummaryDto> RunAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);
        var controlResult = await _declarationControlService.ControlAsync(declarationId, cancellationToken);

        var run = new ValidationRun
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Status = ValidationRunStatus.Completed,
            BlockingCount = controlResult.BlockingAnomaliesCount,
            WarningCount = controlResult.WarningAnomaliesCount,
            InfoCount = controlResult.InfoAnomaliesCount,
            Score = CalculateScore(controlResult.BlockingAnomaliesCount, controlResult.WarningAnomaliesCount),
            CreatedBy = GetAuditUserName()
        };

        foreach (var anomaly in controlResult.Anomalies.Where(x => !x.IsResolved))
        {
            run.Results.Add(new ValidationResult
            {
                Id = Guid.NewGuid(),
                ValidationRunId = run.Id,
                DeclarationId = declarationId,
                AnnexCode = null,
                LineId = anomaly.EntityId,
                Severity = ParseSeverity(anomaly.Severity),
                Code = anomaly.Code,
                Message = anomaly.Message,
                FieldName = null,
                Status = ValidationResultStatus.Open,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        Db.ValidationRuns.Add(run);
        AddAudit("VALIDATION_RUN_CREATED", nameof(ValidationRun), run.Id.ToString(), $"Validation declaration : score {run.Score}.");
        AddEvent(declarationId, "VALIDATION_RUN_CREATED", $"Validation executee : score {run.Score}.");
        await Db.SaveChangesAsync(cancellationToken);

        return new ValidationRunSummaryDto
        {
            Run = ToRunDto(run),
            Results = run.Results.Select(ToResultDto).ToList()
        };
    }

    public async Task<IReadOnlyList<ValidationResultDto>> GetResultsAsync(
        Guid declarationId,
        bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        await GetDeclarationAsync(declarationId, cancellationToken);

        var query = Db.ValidationResults
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId);

        if (!includeResolved)
        {
            query = query.Where(x => x.Status == ValidationResultStatus.Open);
        }

        var results = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return results.Select(ToResultDto).ToList();
    }

    public async Task<ValidationResultDto> MarkCorrectedAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await GetResultAsync(id, cancellationToken);
        result.Status = ValidationResultStatus.Corrected;
        result.ResolvedAt = DateTimeOffset.UtcNow;
        AddAudit("VALIDATION_RESULT_CORRECTED", nameof(ValidationResult), result.Id.ToString(), result.Code);
        await Db.SaveChangesAsync(cancellationToken);

        return ToResultDto(result);
    }

    public async Task<ValidationResultDto> IgnoreAsync(
        Guid id,
        IgnoreValidationResultRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            throw new ApplicationConflictException("Justification obligatoire pour ignorer une anomalie.");
        }

        var result = await GetResultAsync(id, cancellationToken);
        result.Status = ValidationResultStatus.Ignored;
        result.Justification = request.Justification.Trim();
        result.ResolvedAt = DateTimeOffset.UtcNow;
        AddAudit("VALIDATION_RESULT_IGNORED", nameof(ValidationResult), result.Id.ToString(), result.Code);
        await Db.SaveChangesAsync(cancellationToken);

        return ToResultDto(result);
    }

    private async Task<ValidationResult> GetResultAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Db.ValidationResults
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Resultat de validation introuvable.");
    }

    private static int CalculateScore(int blockingCount, int warningCount)
    {
        return Math.Max(0, 100 - (blockingCount * 20) - (warningCount * 5));
    }

    private static DeclarationAnomalySeverity ParseSeverity(string severity)
    {
        return Enum.TryParse<DeclarationAnomalySeverity>(severity, true, out var parsed)
            ? parsed
            : DeclarationAnomalySeverity.Info;
    }

    private static ValidationRunDto ToRunDto(ValidationRun run)
    {
        return new ValidationRunDto
        {
            Id = run.Id,
            DeclarationId = run.DeclarationId,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            Status = run.Status.ToString(),
            BlockingCount = run.BlockingCount,
            WarningCount = run.WarningCount,
            InfoCount = run.InfoCount,
            Score = run.Score,
            CreatedBy = run.CreatedBy
        };
    }

    private static ValidationResultDto ToResultDto(ValidationResult result)
    {
        return new ValidationResultDto
        {
            Id = result.Id,
            ValidationRunId = result.ValidationRunId,
            DeclarationId = result.DeclarationId,
            AnnexCode = result.AnnexCode,
            LineId = result.LineId,
            Severity = result.Severity.ToString(),
            Code = result.Code,
            Message = result.Message,
            FieldName = result.FieldName,
            Status = result.Status.ToString(),
            Justification = result.Justification,
            CreatedAt = result.CreatedAt,
            ResolvedAt = result.ResolvedAt
        };
    }
}
