using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.FiscalEngine;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DeclarationControlService : DeclarationServiceBase, IDeclarationControlService
{
    private static readonly HashSet<string> ControlCodes =
    [
        "LINE_GROSS_AMOUNT_NEGATIVE",
        "LINE_TAXABLE_AMOUNT_NEGATIVE",
        "LINE_WITHHELD_AMOUNT_NEGATIVE",
        "LINE_RATE_INVALID",
        "LINE_WITHHELD_EXCEEDS_TAXABLE",
        "LINE_BENEFICIARY_REQUIRED",
        "LINE_OPERATION_TYPE_REQUIRED",
        "LINE_PAYMENT_DATE_OUT_OF_YEAR",
        "LINE_RATE_ZERO",
        "LINE_DOCUMENT_REFERENCE_MISSING",
        "LINE_ZERO_TAXABLE_WITH_WITHHELD",
        "LINE_FISCAL_CATEGORY_MISSING"
    ];

    private readonly IFiscalControlEngine _fiscalControlEngine;
    private readonly IDeclarationAnomaliesService _declarationAnomaliesService;

    public DeclarationControlService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IFiscalControlEngine fiscalControlEngine,
        IDeclarationAnomaliesService declarationAnomaliesService)
        : base(db, currentUserService, environment)
    {
        _fiscalControlEngine = fiscalControlEngine;
        _declarationAnomaliesService = declarationAnomaliesService;
    }

    public async Task<DeclarationControlResultDto> ControlAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var declaration = await Db.Declarations
            .FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Declaration introuvable.");

        if (declaration.Status is DeclarationStatus.Closed or DeclarationStatus.Archived)
        {
            throw new ApplicationConflictException("Impossible de controler une declaration cloturee ou archivee.");
        }

        var lines = await Db.DeclarationLines
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var beneficiaries = await Db.DeclarationBeneficiaries
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var context = new FiscalControlContext
        {
            DeclarationId = declarationId,
            FiscalYear = declaration.Year,
            Lines = lines.Select(line =>
            {
                beneficiaries.TryGetValue(line.BeneficiaryId ?? Guid.Empty, out var beneficiary);

                return new FiscalControlLine
                {
                    LineId = line.Id,
                    BeneficiaryId = line.BeneficiaryId,
                    BeneficiaryIdentifier = beneficiary?.Identifier,
                    BeneficiaryName = beneficiary?.FullNameOrCompanyName,
                    OperationType = line.OperationType,
                    FiscalCategory = line.FiscalCategory,
                    GrossAmount = line.GrossAmount,
                    TaxableAmount = line.TaxableAmount,
                    Rate = line.Rate,
                    WithheldAmount = line.WithheldAmount,
                    PaymentDate = line.PaymentDate,
                    DocumentReference = line.DocumentReference
                };
            }).ToList()
        };

        var controlResult = _fiscalControlEngine.Run(context);
        var transaction = Db.Database.IsRelational()
            ? await Db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var previousUnresolved = await Db.DeclarationAnomalies
                .Where(x => x.DeclarationId == declarationId &&
                            !x.IsResolved &&
                            ControlCodes.Contains(x.Code))
                .ToListAsync(cancellationToken);

            if (previousUnresolved.Count > 0)
            {
                Db.DeclarationAnomalies.RemoveRange(previousUnresolved);
            }

            foreach (var issue in controlResult.Issues)
            {
                Db.DeclarationAnomalies.Add(new DeclarationAnomaly
                {
                    Id = Guid.NewGuid(),
                    DeclarationId = declarationId,
                    Severity = MapSeverity(issue.Severity),
                    Code = issue.Code,
                    Message = issue.Message,
                    EntityName = issue.EntityName,
                    EntityId = issue.EntityId,
                    IsResolved = false,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            if (controlResult.BlockingIssuesCount == 0)
            {
                if (declaration.Status is not DeclarationStatus.Generated and not DeclarationStatus.Archived)
                {
                    declaration.Status = DeclarationStatus.Controlled;
                }
            }
            else if (declaration.Status == DeclarationStatus.Controlled)
            {
                declaration.Status = DeclarationStatus.Imported;
            }

            declaration.UpdatedAt = DateTimeOffset.UtcNow;

            AddEvent(
                declarationId,
                "DECLARATION_CONTROLLED",
                $"Controle execute : {controlResult.CheckedLinesCount} ligne(s), {controlResult.BlockingIssuesCount} bloquante(s), {controlResult.WarningIssuesCount} avertissement(s), {controlResult.InfoIssuesCount} info(s).");
            AddAudit(
                "CONTROL_EXECUTED",
                nameof(EmployerDeclaration),
                declarationId.ToString(),
                $"Controle execute : {controlResult.CheckedLinesCount} ligne(s), {controlResult.BlockingIssuesCount} bloquante(s), {controlResult.WarningIssuesCount} avertissement(s), {controlResult.InfoIssuesCount} info(s).");

            await Db.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            var anomalies = await GetAnomaliesAsync(declarationId, severity: null, includeResolved: true, cancellationToken);
            return new DeclarationControlResultDto
            {
                DeclarationId = declarationId,
                CheckedLinesCount = controlResult.CheckedLinesCount,
                BlockingAnomaliesCount = anomalies.Count(x => x.Severity == DeclarationAnomalySeverity.Blocking.ToString() && !x.IsResolved),
                WarningAnomaliesCount = anomalies.Count(x => x.Severity == DeclarationAnomalySeverity.Warning.ToString() && !x.IsResolved),
                InfoAnomaliesCount = anomalies.Count(x => x.Severity == DeclarationAnomalySeverity.Info.ToString() && !x.IsResolved),
                DeclarationStatus = declaration.Status.ToString(),
                Anomalies = anomalies
            };
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public Task<IReadOnlyList<DeclarationAnomalyDto>> GetAnomaliesAsync(
        Guid declarationId,
        string? severity,
        bool includeResolved,
        CancellationToken cancellationToken = default)
    {
        return _declarationAnomaliesService.GetByDeclarationAsync(
            declarationId,
            severity,
            includeResolved,
            cancellationToken);
    }

    public Task<DeclarationAnomalyDto> ResolveAnomalyAsync(
        Guid declarationId,
        Guid anomalyId,
        ResolveDeclarationAnomalyRequest request,
        CancellationToken cancellationToken = default)
    {
        return _declarationAnomaliesService.ResolveAsync(
            declarationId,
            anomalyId,
            request,
            cancellationToken);
    }

    private static DeclarationAnomalySeverity MapSeverity(FiscalControlSeverity severity)
    {
        return severity switch
        {
            FiscalControlSeverity.Blocking => DeclarationAnomalySeverity.Blocking,
            FiscalControlSeverity.Warning => DeclarationAnomalySeverity.Warning,
            _ => DeclarationAnomalySeverity.Info
        };
    }
}
