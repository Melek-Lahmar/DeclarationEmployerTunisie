using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Import;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Import;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DeclarationImportService : DeclarationServiceBase, IDeclarationImportService
{
    private readonly IExcelDeclarationImportService _excelImportService;
    private readonly ITemporaryFileStorageService _temporaryFileStorageService;

    public DeclarationImportService(
        ApplicationDbContext db,
        Application.Auth.ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IExcelDeclarationImportService excelImportService,
        ITemporaryFileStorageService temporaryFileStorageService)
        : base(db, currentUserService, environment)
    {
        _excelImportService = excelImportService;
        _temporaryFileStorageService = temporaryFileStorageService;
    }

    public async Task<ExcelImportPreviewDto> PreviewAsync(
        Guid declarationId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var token = await _temporaryFileStorageService.SaveTemporaryImportFileAsync(
            fileStream,
            fileName,
            cancellationToken);

        var temporaryPath = await _temporaryFileStorageService.GetTemporaryImportFilePathAsync(token, cancellationToken);
        await using var readStream = File.OpenRead(temporaryPath);
        var result = await _excelImportService.ParseAsync(readStream, cancellationToken);

        var preview = new ExcelImportPreviewDto
        {
            DeclarationId = declarationId,
            TemporaryFileToken = token,
            TotalRows = result.Rows.Count,
            ValidRows = result.Rows.Count(x => x.IsValid),
            InvalidRows = result.Rows.Count(x => !x.IsValid),
            Rows = result.Rows.Select(ToRowDto).ToList(),
            Errors = result.Issues.Select(ToErrorDto).ToList()
        };

        AddAudit(
            "IMPORT_PREVIEWED",
            nameof(EmployerDeclaration),
            declarationId.ToString(),
            $"Previsualisation import Excel : {preview.ValidRows} ligne(s) valide(s), {preview.InvalidRows} invalide(s).");
        AddEvent(
            declarationId,
            "IMPORT_PREVIEWED",
            $"Previsualisation import Excel : {preview.ValidRows} ligne(s) valide(s), {preview.InvalidRows} invalide(s).");

        await Db.SaveChangesAsync(cancellationToken);

        return preview;
    }

    public async Task<ExcelImportCommitResultDto> CommitAsync(
        Guid declarationId,
        ExcelImportCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        await GetEditableDeclarationAsync(declarationId, cancellationToken);

        var path = await _temporaryFileStorageService.GetTemporaryImportFilePathAsync(
            request.TemporaryFileToken,
            cancellationToken);

        ExcelImportParseResult parsed;
        await using (var readStream = File.OpenRead(path))
        {
            parsed = await _excelImportService.ParseAsync(readStream, cancellationToken);
        }

        var validRows = parsed.Rows.Where(x => x.IsValid).ToList();
        var invalidRows = parsed.Rows.Where(x => !x.IsValid).ToList();

        if (invalidRows.Count > 0 && !request.ImportOnlyValidRows)
        {
            throw new ApplicationConflictException("Le fichier contient des lignes invalides. Importez uniquement les lignes valides ou corrigez le fichier.");
        }

        var result = new ExcelImportCommitResultDto
        {
            DeclarationId = declarationId,
            ImportedRows = validRows.Count,
            SkippedRows = invalidRows.Count
        };

        var transaction = Db.Database.IsRelational()
            ? await Db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            foreach (var parsedRow in validRows)
            {
                var beneficiary = await GetOrCreateBeneficiaryAsync(declarationId, parsedRow, cancellationToken);
                if (beneficiary.Created)
                {
                    result.CreatedBeneficiaries++;
                }

                Db.DeclarationLines.Add(new DeclarationLine
                {
                    Id = Guid.NewGuid(),
                    DeclarationId = declarationId,
                    BeneficiaryId = beneficiary.Entity.Id,
                    OperationType = parsedRow.OperationType!,
                    FiscalCategory = parsedRow.FiscalCategory,
                    GrossAmount = parsedRow.GrossAmount ?? 0m,
                    TaxableAmount = parsedRow.TaxableAmount ?? 0m,
                    Rate = parsedRow.Rate ?? 0m,
                    WithheldAmount = parsedRow.WithheldAmount ?? 0m,
                    PaymentDate = parsedRow.PaymentDate,
                    DocumentReference = parsedRow.DocumentReference,
                    Notes = parsedRow.Notes,
                    Status = DeclarationLineStatus.Imported,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                result.CreatedLines++;
            }

            foreach (var invalidRow in invalidRows)
            {
                var rowIssues = parsed.Issues
                    .Where(x => x.RowNumber == invalidRow.RowNumber)
                    .Select(x => x.Message)
                    .Distinct()
                    .ToList();

                Db.DeclarationAnomalies.Add(new DeclarationAnomaly
                {
                    Id = Guid.NewGuid(),
                    DeclarationId = declarationId,
                    Severity = DeclarationAnomalySeverity.Warning,
                    Code = "EXCEL_IMPORT_INVALID_ROW",
                    Message = $"Ligne Excel {invalidRow.RowNumber} ignoree : {string.Join(" | ", rowIssues)}",
                    EntityName = "ExcelImportRow",
                    EntityId = invalidRow.RowNumber.ToString(),
                    IsResolved = false,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                result.CreatedAnomalies++;
            }

            if (result.CreatedLines > 0)
            {
                var declaration = await Db.Declarations.FirstAsync(x => x.Id == declarationId, cancellationToken);
                declaration.Status = DeclarationStatus.Imported;
                declaration.UpdatedAt = DateTimeOffset.UtcNow;
            }

            AddAudit(
                "IMPORT_COMPLETED",
                nameof(EmployerDeclaration),
                declarationId.ToString(),
                $"Import Excel termine : {result.CreatedLines} ligne(s), {result.CreatedBeneficiaries} beneficiaire(s), {result.CreatedAnomalies} anomalie(s).");
            AddEvent(
                declarationId,
                "IMPORT_COMPLETED",
                $"Import Excel termine : {result.CreatedLines} ligne(s), {result.CreatedBeneficiaries} beneficiaire(s), {result.CreatedAnomalies} anomalie(s).");

            await Db.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            await _temporaryFileStorageService.DeleteTemporaryImportFileAsync(request.TemporaryFileToken, cancellationToken);
            return result;
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

    private async Task<(DeclarationBeneficiary Entity, bool Created)> GetOrCreateBeneficiaryAsync(
        Guid declarationId,
        ExcelImportParsedRow parsedRow,
        CancellationToken cancellationToken)
    {
        var identifier = parsedRow.BeneficiaryIdentifier!;
        var existing = await Db.DeclarationBeneficiaries
            .FirstOrDefaultAsync(
                x => x.DeclarationId == declarationId && x.Identifier == identifier,
                cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        Enum.TryParse<BeneficiaryIdentifierType>(parsedRow.BeneficiaryIdentifierType, true, out var identifierType);
        var entity = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            IdentifierType = identifierType,
            Identifier = identifier,
            FullNameOrCompanyName = parsedRow.BeneficiaryName!,
            Address = parsedRow.Address,
            Country = parsedRow.Country,
            IsResident = parsedRow.IsResident ?? true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Db.DeclarationBeneficiaries.Add(entity);
        return (entity, true);
    }

    private static ExcelImportRowDto ToRowDto(ExcelImportParsedRow row) => new()
    {
        RowNumber = row.RowNumber,
        IsValid = row.IsValid,
        BeneficiaryIdentifierType = row.BeneficiaryIdentifierType,
        BeneficiaryIdentifier = row.BeneficiaryIdentifier,
        BeneficiaryName = row.BeneficiaryName,
        OperationType = row.OperationType,
        FiscalCategory = row.FiscalCategory,
        GrossAmount = row.GrossAmount,
        TaxableAmount = row.TaxableAmount,
        Rate = row.Rate,
        WithheldAmount = row.WithheldAmount,
        PaymentDate = row.PaymentDate,
        DocumentReference = row.DocumentReference,
        Notes = row.Notes
    };

    private static ExcelImportErrorDto ToErrorDto(ExcelImportValidationIssue issue) => new()
    {
        RowNumber = issue.RowNumber,
        ColumnName = issue.ColumnName,
        Code = issue.Code,
        Message = issue.Message
    };
}
