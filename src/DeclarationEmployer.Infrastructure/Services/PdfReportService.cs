using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Reports;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class PdfReportService : IPdfReportService
{
    private readonly ApplicationDbContext _db;

    public PdfReportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(byte[] Content, string FileName)> BuildDeclarationSummaryAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadBaseAsync(declarationId, cancellationToken);
        var linesCount = await _db.DeclarationLines.CountAsync(x => x.DeclarationId == declarationId, cancellationToken);
        var anomaliesCount = await _db.DeclarationAnomalies.CountAsync(x => x.DeclarationId == declarationId && !x.IsResolved, cancellationToken);

        var pdf = PdfReportBuilder.BuildSimpleReport(
            "Resume declaration",
            BuildFacts(data),
            [$"Lignes : {linesCount}", $"Anomalies ouvertes : {anomaliesCount}", $"Statut : {data.Declaration.Status}"]);

        return (pdf, $"rapport_resume_{data.Client.Code}_{data.FiscalYear.Year}.pdf");
    }

    public async Task<(byte[] Content, string FileName)> BuildAnnexA1Async(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadBaseAsync(declarationId, cancellationToken);
        var lines = await _db.DeclarationLines
            .Include(x => x.Annex)
            .Include(x => x.Beneficiary)
            .Where(x => x.DeclarationId == declarationId && x.Annex != null && x.Annex.AnnexCode == "A1")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var pdf = PdfReportBuilder.BuildSimpleReport(
            "Annexe I foundation",
            BuildFacts(data),
            lines.Count == 0
                ? ["Aucune ligne Annexe I."]
                : lines.Select(x => $"{x.Beneficiary?.Identifier} - {x.Beneficiary?.FullNameOrCompanyName} - Brut {x.GrossAmount:0.000} - Retenue {x.WithheldAmount:0.000}").ToList());

        return (pdf, $"rapport_annexe_a1_{data.Client.Code}_{data.FiscalYear.Year}.pdf");
    }

    public async Task<(byte[] Content, string FileName)> BuildValidationErrorsAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadBaseAsync(declarationId, cancellationToken);
        var anomalies = await _db.ValidationResults
            .Where(x => x.DeclarationId == declarationId && x.Status == ValidationResultStatus.Open)
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var pdf = PdfReportBuilder.BuildSimpleReport(
            "Anomalies de validation",
            BuildFacts(data),
            anomalies.Count == 0
                ? ["Aucune anomalie ouverte."]
                : anomalies.Select(x => $"{x.Severity} - {x.Code} - {x.Message}").ToList());

        return (pdf, $"rapport_anomalies_{data.Client.Code}_{data.FiscalYear.Year}.pdf");
    }

    public async Task<(byte[] Content, string FileName)> BuildGenerationReportAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var data = await LoadBaseAsync(declarationId, cancellationToken);
        var files = await _db.GeneratedFiles
            .Where(x => x.DeclarationId == declarationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var pdf = PdfReportBuilder.BuildSimpleReport(
            "Rapport generation",
            BuildFacts(data),
            files.Count == 0
                ? ["Aucun fichier genere."]
                : files.Select(x => $"{x.FileType} - {x.FileName} - SHA256 {x.Sha256Hash}").ToList());

        return (pdf, $"rapport_generation_{data.Client.Code}_{data.FiscalYear.Year}.pdf");
    }

    private async Task<ReportBaseData> LoadBaseAsync(Guid declarationId, CancellationToken cancellationToken)
    {
        var declaration = await _db.Declarations
            .FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Declaration introuvable.");
        var client = await _db.Clients.FirstAsync(x => x.Id == declaration.ClientCompanyId, cancellationToken);
        var fiscalYear = await _db.FiscalYears.FirstAsync(x => x.Id == declaration.FiscalYearId, cancellationToken);

        return new ReportBaseData(declaration, client, fiscalYear);
    }

    private static List<(string Label, string Value)> BuildFacts(ReportBaseData data)
    {
        return
        [
            ("Client", $"{data.Client.Code} - {data.Client.RaisonSociale}"),
            ("Exercice", data.FiscalYear.Year.ToString()),
            ("Declaration", data.Declaration.Title),
            ("Limite fiscale", FiscalReferenceSeedService.OfficialMappingIncompleteMessage)
        ];
    }

    private sealed record ReportBaseData(
        Domain.Declarations.EmployerDeclaration Declaration,
        Domain.Cabinet.ClientCompany Client,
        Domain.Cabinet.FiscalYear FiscalYear);
}
