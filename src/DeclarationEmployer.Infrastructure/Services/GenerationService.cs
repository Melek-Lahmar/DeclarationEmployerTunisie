using System.Globalization;
using System.Text;
using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Generation;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class GenerationService : DeclarationServiceBase, IGenerationService
{
    private readonly IDeclarationExportStorageService _storageService;
    private readonly IFileHashService _fileHashService;

    public GenerationService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IDeclarationExportStorageService storageService,
        IFileHashService fileHashService)
        : base(db, currentUserService, environment)
    {
        _storageService = storageService;
        _fileHashService = fileHashService;
    }

    public async Task<GenerationResultDto> GenerateAsync(
        Guid declarationId,
        GenerateDeclarationFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OfficialModeRequested)
        {
            throw new ApplicationConflictException(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
        }

        var data = await LoadDataAsync(declarationId, cancellationToken);
        EnsureCanGenerate(data);

        var decempName = BuildFileName("DECEMP_25_FOUNDATION", data.Client.Code, data.FiscalYear.Year);
        var annexName = BuildFileName("ANXEMP_FOUNDATION", data.Client.Code, data.FiscalYear.Year);

        var decempContent = BuildDecempFoundation(data);
        var annexContent = BuildAnnexFoundation(data);
        var files = new List<GenerationFileDto>
        {
            await SaveGeneratedFileAsync(data, decempName, decempContent, GeneratedFileType.FoundationDecemp, cancellationToken),
            await SaveGeneratedFileAsync(data, annexName, annexContent, GeneratedFileType.FoundationAnnex, cancellationToken)
        };

        data.Declaration.Status = DeclarationStatus.Generated;
        data.Declaration.UpdatedAt = DateTimeOffset.UtcNow;
        AddEvent(declarationId, "FOUNDATION_GENERATION_CREATED", "Generation foundation non officielle creee.");
        AddAudit("FOUNDATION_GENERATION_CREATED", nameof(EmployerDeclaration), declarationId.ToString(), "Generation foundation non officielle creee.");
        await Db.SaveChangesAsync(cancellationToken);

        return new GenerationResultDto
        {
            DeclarationId = declarationId,
            IsOfficialMode = false,
            Message = FiscalReferenceSeedService.OfficialMappingIncompleteMessage,
            DeclarationStatus = data.Declaration.Status.ToString(),
            Files = files
        };
    }

    private async Task<GenerationData> LoadDataAsync(Guid declarationId, CancellationToken cancellationToken)
    {
        var declaration = await Db.Declarations
            .FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Declaration introuvable.");

        var client = await Db.Clients.FirstAsync(x => x.Id == declaration.ClientCompanyId, cancellationToken);
        var fiscalYear = await Db.FiscalYears.FirstAsync(x => x.Id == declaration.FiscalYearId, cancellationToken);
        var lines = await Db.DeclarationLines
            .Include(x => x.Annex)
            .Include(x => x.Beneficiary)
            .Where(x => x.DeclarationId == declarationId && x.Status != DeclarationLineStatus.Excluded)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var blockingCount = await Db.DeclarationAnomalies
            .CountAsync(x => x.DeclarationId == declarationId && !x.IsResolved && x.Severity == DeclarationAnomalySeverity.Blocking, cancellationToken);

        return new GenerationData(declaration, client, fiscalYear, lines, blockingCount);
    }

    private static void EnsureCanGenerate(GenerationData data)
    {
        if (data.Declaration.Status is DeclarationStatus.Closed or DeclarationStatus.Archived)
        {
            throw new ApplicationConflictException("Impossible de generer une declaration cloturee ou archivee.");
        }

        if (data.BlockingAnomaliesCount > 0)
        {
            throw new ApplicationConflictException("Des anomalies bloquantes non resolues empechent la generation.");
        }

        if (data.Lines.Count == 0)
        {
            throw new ApplicationConflictException("Impossible de generer sans lignes de declaration.");
        }
    }

    private async Task<GenerationFileDto> SaveGeneratedFileAsync(
        GenerationData data,
        string fileName,
        string content,
        GeneratedFileType fileType,
        CancellationToken cancellationToken)
    {
        var saved = await _storageService.SaveExportAsync(
            data.Client.Code,
            data.FiscalYear.Year,
            fileName,
            content,
            cancellationToken);
        var hash = await _fileHashService.ComputeSha256Async(saved.AbsolutePath, cancellationToken);
        var entity = new GeneratedFile
        {
            Id = Guid.NewGuid(),
            DeclarationId = data.Declaration.Id,
            FileType = fileType,
            FileName = fileName,
            RelativePath = saved.RelativePath,
            Sha256Hash = hash,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = GetAuditUserName()
        };

        Db.GeneratedFiles.Add(entity);

        return new GenerationFileDto
        {
            GeneratedFileId = entity.Id,
            FileName = entity.FileName,
            RelativePath = entity.RelativePath,
            Sha256Hash = entity.Sha256Hash,
            FileType = entity.FileType.ToString()
        };
    }

    private static string BuildDecempFoundation(GenerationData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("FOUNDATION_NON_OFFICIEL");
        builder.AppendLine(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
        builder.AppendLine($"CLIENT|{Sanitize(data.Client.Code)}|{Sanitize(data.Client.RaisonSociale)}");
        builder.AppendLine($"YEAR|{data.FiscalYear.Year.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"DECLARATION|{data.Declaration.Id}|{Sanitize(data.Declaration.Title)}");
        builder.AppendLine($"LINES|{data.Lines.Count.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"GROSS|{data.Lines.Sum(x => x.GrossAmount).ToString("0.000", CultureInfo.InvariantCulture)}");
        builder.AppendLine($"WITHHELD|{data.Lines.Sum(x => x.WithheldAmount).ToString("0.000", CultureInfo.InvariantCulture)}");
        return builder.ToString();
    }

    private static string BuildAnnexFoundation(GenerationData data)
    {
        var builder = new StringBuilder();
        builder.AppendLine("FOUNDATION_NON_OFFICIEL");
        builder.AppendLine(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
        foreach (var line in data.Lines)
        {
            builder.AppendLine(string.Join(
                "|",
                Sanitize(line.Annex?.AnnexCode ?? "NA"),
                Sanitize(line.Beneficiary?.Identifier ?? string.Empty),
                Sanitize(line.Beneficiary?.FullNameOrCompanyName ?? string.Empty),
                Sanitize(line.OperationType),
                line.GrossAmount.ToString("0.000", CultureInfo.InvariantCulture),
                line.TaxableAmount.ToString("0.000", CultureInfo.InvariantCulture),
                line.WithheldAmount.ToString("0.000", CultureInfo.InvariantCulture)));
        }

        return builder.ToString();
    }

    private static string BuildFileName(string prefix, string clientCode, int year)
    {
        return $"{prefix}_{Sanitize(clientCode)}_{year}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
    }

    private static string Sanitize(string value)
    {
        var sanitized = new string(value.Select(ch => ch is >= (char)32 and <= (char)126 && ch != '|' ? ch : '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "NA" : sanitized.Trim();
    }

    private sealed record GenerationData(
        EmployerDeclaration Declaration,
        ClientCompany Client,
        FiscalYear FiscalYear,
        List<DeclarationLine> Lines,
        int BlockingAnomaliesCount);
}
