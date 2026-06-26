using System.Text;
using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Archive;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class ArchiveService : DeclarationServiceBase, IArchiveService
{
    private readonly IDeclarationExportStorageService _storageService;
    private readonly IFileHashService _fileHashService;

    public ArchiveService(
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

    public async Task<ArchiveDeclarationResultDto> ArchiveAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var declaration = await Db.Declarations
            .FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Declaration introuvable.");

        if (declaration.Status == DeclarationStatus.Archived)
        {
            throw new ApplicationConflictException("Declaration deja archivee.");
        }

        var client = await Db.Clients.FirstAsync(x => x.Id == declaration.ClientCompanyId, cancellationToken);
        var fiscalYear = await Db.FiscalYears.FirstAsync(x => x.Id == declaration.FiscalYearId, cancellationToken);
        var generatedFiles = await Db.GeneratedFiles
            .Where(x => x.DeclarationId == declarationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var fileName = $"ARCHIVE_RECEIPT_{Sanitize(client.Code)}_{fiscalYear.Year}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
        var content = BuildReceipt(declaration, client.Code, client.RaisonSociale, fiscalYear.Year, generatedFiles);
        var saved = await _storageService.SaveExportAsync(client.Code, fiscalYear.Year, fileName, content, cancellationToken);
        var hash = await _fileHashService.ComputeSha256Async(saved.AbsolutePath, cancellationToken);
        var archivedAt = DateTimeOffset.UtcNow;
        var archive = new ArchivedDocument
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            ClientCompanyId = client.Id,
            FiscalYearId = fiscalYear.Id,
            DocumentType = ArchivedDocumentType.Other,
            FileName = fileName,
            RelativePath = saved.RelativePath,
            Sha256Hash = hash,
            ArchivedAt = archivedAt,
            ArchivedBy = GetAuditUserName()
        };

        Db.ArchivedDocuments.Add(archive);
        declaration.Status = DeclarationStatus.Archived;
        declaration.IsLocked = true;
        declaration.UpdatedAt = archivedAt;
        AddEvent(declarationId, "DECLARATION_ARCHIVED", "Archivage foundation cree.");
        AddAudit("DECLARATION_ARCHIVED", nameof(EmployerDeclaration), declarationId.ToString(), "Archivage foundation cree.");
        await Db.SaveChangesAsync(cancellationToken);

        return new ArchiveDeclarationResultDto
        {
            DeclarationId = declarationId,
            ArchivedDocumentId = archive.Id,
            FileName = archive.FileName,
            RelativePath = archive.RelativePath,
            Sha256Hash = archive.Sha256Hash,
            DeclarationStatus = declaration.Status.ToString(),
            ArchivedAt = archive.ArchivedAt
        };
    }

    private static string BuildReceipt(
        EmployerDeclaration declaration,
        string clientCode,
        string clientName,
        int year,
        IReadOnlyList<GeneratedFile> generatedFiles)
    {
        var builder = new StringBuilder();
        builder.AppendLine("ARCHIVE_RECEIPT_FOUNDATION");
        builder.AppendLine(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
        builder.AppendLine($"DeclarationId={declaration.Id}");
        builder.AppendLine($"Client={clientCode} - {clientName}");
        builder.AppendLine($"Year={year}");
        builder.AppendLine($"GeneratedFiles={generatedFiles.Count}");
        foreach (var file in generatedFiles)
        {
            builder.AppendLine($"{file.FileType}|{file.FileName}|{file.Sha256Hash}");
        }

        return builder.ToString();
    }

    private static string Sanitize(string value)
    {
        var sanitized = new string(value.Trim().Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "CLIENT" : sanitized;
    }
}
