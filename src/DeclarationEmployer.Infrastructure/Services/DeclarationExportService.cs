using System.Globalization;
using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DeclarationExportService : DeclarationServiceBase, IDeclarationExportService
{
    private readonly IDeclarationExportStorageService _exportStorageService;
    private readonly IFileHashService _fileHashService;
    private readonly IInternalDeclarationCsvGenerator _csvGenerator;
    private readonly IGeneratedFilesService _generatedFilesService;

    public DeclarationExportService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IDeclarationExportStorageService exportStorageService,
        IFileHashService fileHashService,
        IInternalDeclarationCsvGenerator csvGenerator,
        IGeneratedFilesService generatedFilesService)
        : base(db, currentUserService, environment)
    {
        _exportStorageService = exportStorageService;
        _fileHashService = fileHashService;
        _csvGenerator = csvGenerator;
        _generatedFilesService = generatedFilesService;
    }

    public async Task<DeclarationExportPreviewDto> PreviewAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var data = await LoadExportDataAsync(declarationId, cancellationToken);
        var exportableLines = data.Lines.Where(IsExportableLine).ToList();
        var blockingAnomalies = data.Anomalies.Where(x => !x.IsResolved && x.Severity == DeclarationAnomalySeverity.Blocking).ToList();
        var warningAnomalies = data.Anomalies.Where(x => !x.IsResolved && x.Severity == DeclarationAnomalySeverity.Warning).ToList();
        var infoAnomalies = data.Anomalies.Where(x => !x.IsResolved && x.Severity == DeclarationAnomalySeverity.Info).ToList();

        var blockingMessages = new List<string>();
        if (exportableLines.Count == 0)
        {
            blockingMessages.Add("Aucune ligne de declaration exportable.");
        }

        if (data.Declaration.Status is DeclarationStatus.Closed or DeclarationStatus.Archived)
        {
            blockingMessages.Add("Impossible de generer un export pour une declaration cloturee ou archivee.");
        }

        blockingMessages.AddRange(blockingAnomalies.Select(x => x.Message).Distinct());

        return new DeclarationExportPreviewDto
        {
            DeclarationId = data.Declaration.Id,
            LinesCount = exportableLines.Count,
            BlockingAnomaliesCount = blockingAnomalies.Count,
            WarningAnomaliesCount = warningAnomalies.Count,
            InfoAnomaliesCount = infoAnomalies.Count,
            TotalGrossAmount = exportableLines.Sum(x => x.GrossAmount),
            TotalTaxableAmount = exportableLines.Sum(x => x.TaxableAmount),
            TotalWithheldAmount = exportableLines.Sum(x => x.WithheldAmount),
            CanGenerate = blockingMessages.Count == 0,
            BlockingMessages = blockingMessages
        };
    }

    public async Task<DeclarationExportResultDto> GenerateAsync(
        Guid declarationId,
        GenerateDeclarationExportRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureCsvFormat(request.Format);

        var data = await LoadExportDataAsync(declarationId, cancellationToken);
        var exportableLines = data.Lines.Where(IsExportableLine).ToList();
        var blockingAnomalies = data.Anomalies
            .Where(x => !x.IsResolved && x.Severity == DeclarationAnomalySeverity.Blocking)
            .ToList();

        if (data.Declaration.Status is DeclarationStatus.Closed or DeclarationStatus.Archived)
        {
            throw new ApplicationConflictException("Impossible de generer un export pour une declaration cloturee ou archivee.");
        }

        if (exportableLines.Count == 0)
        {
            throw new ApplicationConflictException("Impossible de generer un export sans lignes de declaration.");
        }

        if (blockingAnomalies.Count > 0)
        {
            throw new ApplicationConflictException("Des anomalies bloquantes non resolues empechent la generation.");
        }

        var document = new InternalDeclarationCsvDocument
        {
            DeclarationId = data.Declaration.Id,
            ClientCode = data.Client.Code,
            ClientName = data.Client.RaisonSociale,
            FiscalYear = data.FiscalYear.Year,
            Lines = exportableLines.Select(line =>
            {
                data.Beneficiaries.TryGetValue(line.BeneficiaryId ?? Guid.Empty, out var beneficiary);
                return new InternalDeclarationCsvLine
                {
                    LineId = line.Id,
                    BeneficiaryIdentifierType = beneficiary?.IdentifierType,
                    BeneficiaryIdentifier = beneficiary?.Identifier,
                    BeneficiaryName = beneficiary?.FullNameOrCompanyName,
                    OperationType = line.OperationType,
                    FiscalCategory = line.FiscalCategory,
                    GrossAmount = line.GrossAmount,
                    TaxableAmount = line.TaxableAmount,
                    Rate = line.Rate,
                    WithheldAmount = line.WithheldAmount,
                    PaymentDate = line.PaymentDate,
                    DocumentReference = line.DocumentReference,
                    Notes = line.Notes,
                    Status = line.Status
                };
            }).ToList()
        };

        var fileName = BuildFileName(data.Client.Code, data.FiscalYear.Year);
        var content = _csvGenerator.Generate(document);
        var transaction = Db.Database.IsRelational()
            ? await Db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var saved = await _exportStorageService.SaveExportAsync(
                data.Client.Code,
                data.FiscalYear.Year,
                fileName,
                content,
                cancellationToken);

            var hash = await _fileHashService.ComputeSha256Async(saved.AbsolutePath, cancellationToken);
            var generatedFile = new GeneratedFile
            {
                Id = Guid.NewGuid(),
                DeclarationId = data.Declaration.Id,
                FileType = GeneratedFileType.InternalExport,
                FileName = fileName,
                RelativePath = saved.RelativePath,
                Sha256Hash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = GetAuditUserName()
            };

            Db.GeneratedFiles.Add(generatedFile);
            data.Declaration.Status = DeclarationStatus.Generated;
            data.Declaration.UpdatedAt = DateTimeOffset.UtcNow;

            var details = BuildAuditDetails(exportableLines.Count, generatedFile.RelativePath, request.Notes);
            AddEvent(data.Declaration.Id, "INTERNAL_EXPORT_GENERATED", details);
            AddAudit("INTERNAL_EXPORT_GENERATED", nameof(EmployerDeclaration), data.Declaration.Id.ToString(), details);

            await Db.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new DeclarationExportResultDto
            {
                DeclarationId = data.Declaration.Id,
                GeneratedFileId = generatedFile.Id,
                FileName = generatedFile.FileName,
                RelativePath = generatedFile.RelativePath,
                Sha256Hash = generatedFile.Sha256Hash,
                FileType = generatedFile.FileType.ToString(),
                CreatedAt = generatedFile.CreatedAt,
                DeclarationStatus = data.Declaration.Status.ToString(),
                ExportedLinesCount = exportableLines.Count,
                TotalGrossAmount = exportableLines.Sum(x => x.GrossAmount),
                TotalTaxableAmount = exportableLines.Sum(x => x.TaxableAmount),
                TotalWithheldAmount = exportableLines.Sum(x => x.WithheldAmount)
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

    public Task<IReadOnlyList<GeneratedFileDto>> GetGeneratedFilesAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        return _generatedFilesService.GetByDeclarationAsync(declarationId, cancellationToken);
    }

    private async Task<ExportData> LoadExportDataAsync(Guid declarationId, CancellationToken cancellationToken)
    {
        var declaration = await Db.Declarations
            .FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Declaration introuvable.");

        var client = await Db.Clients
            .FirstAsync(x => x.Id == declaration.ClientCompanyId, cancellationToken);
        var fiscalYear = await Db.FiscalYears
            .FirstAsync(x => x.Id == declaration.FiscalYearId, cancellationToken);
        var lines = await Db.DeclarationLines
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var beneficiaries = await Db.DeclarationBeneficiaries
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var anomalies = await Db.DeclarationAnomalies
            .AsNoTracking()
            .Where(x => x.DeclarationId == declarationId)
            .ToListAsync(cancellationToken);

        return new ExportData(declaration, client, fiscalYear, lines, beneficiaries, anomalies);
    }

    private static bool IsExportableLine(DeclarationLine line)
    {
        return line.Status != DeclarationLineStatus.Excluded;
    }

    private static void EnsureCsvFormat(string? format)
    {
        if (!string.IsNullOrWhiteSpace(format) && !string.Equals(format, "CSV", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApplicationConflictException("Le format demande n'est pas pris en charge. Seul CSV est autorise.");
        }
    }

    private static string BuildFileName(string clientCode, int year)
    {
        var safeClientCode = new string(
            clientCode
                .Trim()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
                .ToArray());
        if (string.IsNullOrWhiteSpace(safeClientCode))
        {
            safeClientCode = "CLIENT";
        }

        return $"DET_EXPORT_INTERNE_{safeClientCode}_{year.ToString(CultureInfo.InvariantCulture)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
    }

    private static string BuildAuditDetails(int exportedLinesCount, string relativePath, string? notes)
    {
        var details = $"Export interne structure genere : {exportedLinesCount} ligne(s), chemin relatif '{relativePath}'.";
        if (!string.IsNullOrWhiteSpace(notes))
        {
            details += $" Notes : {notes.Trim()}";
        }

        return details;
    }

    private sealed record ExportData(
        EmployerDeclaration Declaration,
        ClientCompany Client,
        FiscalYear FiscalYear,
        List<DeclarationLine> Lines,
        Dictionary<Guid, DeclarationBeneficiary> Beneficiaries,
        List<DeclarationAnomaly> Anomalies);
}
