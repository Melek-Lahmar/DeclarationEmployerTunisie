using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Generation;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.FiscalEngine.Empcca;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class EmpccaGenerationPreviewService : DeclarationServiceBase, IEmpccaGenerationPreviewService
{
    public EmpccaGenerationPreviewService(
        ApplicationDbContext db,
        ICurrentUserService currentUserService,
        IHostEnvironment environment)
        : base(db, currentUserService, environment)
    {
    }

    public async Task<EmpccaGenerationPreviewDto> PreviewAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var declaration = await Db.Declarations.FirstOrDefaultAsync(x => x.Id == declarationId, cancellationToken)
            ?? throw new Application.Common.ApplicationNotFoundException("Declaration introuvable.");
        var client = await Db.Clients.FirstAsync(x => x.Id == declaration.ClientCompanyId, cancellationToken);
        var lines = await Db.DeclarationLines
            .Include(x => x.Annex).Include(x => x.Beneficiary)
            .Include(x => x.AnnexA1Detail).Include(x => x.AnnexA2Detail).Include(x => x.AnnexA3Detail)
            .Include(x => x.AnnexA4Detail).Include(x => x.AnnexA5Detail).Include(x => x.AnnexA6Detail).Include(x => x.AnnexA7Detail)
            .Where(x => x.DeclarationId == declarationId && x.Status != DeclarationLineStatus.Excluded)
            .OrderBy(x => x.OrderNumber).ToListAsync(cancellationToken);

        var declarant = new EmpccaDeclarant(
            client.MatriculeFiscal ?? string.Empty, client.Cle ?? string.Empty, client.Categorie ?? string.Empty,
            client.Etablissement ?? string.Empty, declaration.Year, (int)declaration.ActCode,
            client.RaisonSociale, client.Activite ?? string.Empty, client.Ville ?? string.Empty,
            client.Adresse ?? string.Empty, client.NumeroAdresse ?? string.Empty, client.CodePostal ?? string.Empty);

        var artifacts = new List<EmpccaGenerationArtifact>();
        AddAnnexArtifacts(declarant, lines, artifacts);
        artifacts.Insert(0, SafeGenerate("DECEMP_25", () => BuildDecemp(declarant, artifacts, lines)));

        var blocking = artifacts.SelectMany(x => x.BlockingIssues).ToList();
        blocking.AddRange(await Db.DeclarationAnomalies
            .Where(x => x.DeclarationId == declarationId && !x.IsResolved && x.Severity == DeclarationAnomalySeverity.Blocking)
            .Select(x => x.Message).ToListAsync(cancellationToken));
        if (declaration.IsLocked || declaration.Status is DeclarationStatus.Closed or DeclarationStatus.Archived)
            blocking.Add("La declaration est verrouillee, cloturee ou archivee.");

        var guard = new EmpccaOfficialGenerationGuard();
        blocking.AddRange(artifacts.SelectMany(guard.Validate));
        blocking = blocking.Distinct(StringComparer.Ordinal).ToList();

        return new EmpccaGenerationPreviewDto
        {
            DeclarationId = declarationId,
            CanGenerateOfficial = blocking.Count == 0,
            BlockingIssues = blocking,
            Files = artifacts.Select(x => new EmpccaGeneratedArtifactDto
            {
                FileName = x.FileName,
                LineCount = x.Lines.Count,
                ExpectedLineLength = x.FileName == "DECEMP_25" ? 38 : 399,
                IsOfficial = x.IsOfficial,
                BlockingIssues = guard.Validate(x)
            }).ToList()
        };
    }

    private static void AddAnnexArtifacts(
        EmpccaDeclarant declarant,
        IReadOnlyList<DeclarationLine> lines,
        ICollection<EmpccaGenerationArtifact> artifacts)
    {
        var a1 = lines.Where(x => x.AnnexA1Detail is not null).Select(x =>
            new EmpccaAnnexA1Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA1Detail!)).ToList();
        var a2 = lines.Where(x => x.AnnexA2Detail is not null).Select(x =>
            new EmpccaAnnexA2Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA2Detail!)).ToList();
        var a3 = lines.Where(x => x.AnnexA3Detail is not null).Select(x =>
            new EmpccaAnnexA3Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA3Detail!)).ToList();
        var a4 = lines.Where(x => x.AnnexA4Detail is not null).Select(x =>
            new EmpccaAnnexA4Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA4Detail!)).ToList();
        var a5 = lines.Where(x => x.AnnexA5Detail is not null).Select(x =>
            new EmpccaAnnexA5Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA5Detail!)).ToList();
        var a6 = lines.Where(x => x.AnnexA6Detail is not null).Select(x =>
            new EmpccaAnnexA6Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA6Detail!)).ToList();
        var a7 = lines.Where(x => x.AnnexA7Detail is not null).Select(x =>
            new EmpccaAnnexA7Record(x.OrderNumber ?? 0, Beneficiary(x), x.AnnexA7Detail!)).ToList();

        if (a1.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_1_25_1", () => new OfficialAnnex1Generator().Generate(declarant, a1)));
        if (a2.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_2_25_1", () => new OfficialAnnex2Generator().Generate(declarant, a2)));
        if (a3.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_3_25_1", () => new OfficialAnnex3Generator().Generate(declarant, a3)));
        if (a4.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_4_25_1", () => new OfficialAnnex4Generator().Generate(declarant, a4)));
        if (a5.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_5_25_1", () => new OfficialAnnex5Generator().Generate(declarant, a5)));
        if (a6.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_6_25_1", () => new OfficialAnnex6Generator().Generate(declarant, a6)));
        if (a7.Count > 0) artifacts.Add(SafeGenerate("ANXEMP_7_25_1", () => new OfficialAnnex7Generator().Generate(declarant, a7)));
    }

    private static EmpccaGenerationArtifact BuildDecemp(
        EmpccaDeclarant declarant,
        IReadOnlyCollection<EmpccaGenerationArtifact> artifacts,
        IReadOnlyCollection<DeclarationLine> lines)
    {
        var deposited = artifacts.Where(x => x.FileName.StartsWith("ANXEMP_", StringComparison.Ordinal))
            .Select(x => int.Parse(x.FileName.AsSpan(7, 1), System.Globalization.CultureInfo.InvariantCulture)).ToHashSet();
        var values = new Dictionary<int, EmpccaDecempRecordValue>();
        var a1 = lines.Where(x => x.AnnexA1Detail is not null).Select(x => x.AnnexA1Detail!).ToList();
        var a5 = lines.Where(x => x.AnnexA5Detail is not null).Select(x => x.AnnexA5Detail!).ToList();
        if (a1.Count > 0)
        {
            values[1] = new(a1.Sum(x => x.GrossTaxableIncome), 0, a1.Sum(x => x.CommonRegimeWithheldAmount));
            values[2] = new(a1.Sum(x => x.GrossTaxableIncome), 0, a1.Sum(x => x.ForeignEmployeeWithheldAmount));
            values[3] = new(a1.Sum(x => x.GrossTaxableIncome), 0, a1.Sum(x => x.SocialSolidarityContribution));
        }
        if (a5.Count > 0)
            values[49] = new(a5.Sum(x => x.PurchasesFromFifteenPercentCompanies), 3,
                a5.Sum(x => x.DeliveryPlatformThreePercentWithheldAmount));
        return new OfficialDecemp2025Generator().Generate(declarant, deposited, values);
    }

    private static EmpccaBeneficiary Beneficiary(DeclarationLine line)
    {
        var beneficiary = line.Beneficiary ?? throw new InvalidOperationException("Beneficiaire EMPCCA absent.");
        var type = beneficiary.IdentifierType switch
        {
            BeneficiaryIdentifierType.MatriculeFiscal => 1,
            BeneficiaryIdentifierType.CIN => 2,
            BeneficiaryIdentifierType.ResidenceCard => 3,
            BeneficiaryIdentifierType.NonDomiciledIdentifier => 4,
            _ => 0
        };
        return new EmpccaBeneficiary(type, beneficiary.Identifier, beneficiary.FullNameOrCompanyName,
            beneficiary.JobTitle ?? beneficiary.Activity ?? string.Empty, beneficiary.Address ?? string.Empty);
    }

    private static EmpccaGenerationArtifact SafeGenerate(string fileName, Func<EmpccaGenerationArtifact> generate)
    {
        try
        {
            return generate();
        }
        catch (ArgumentException exception)
        {
            return new EmpccaGenerationArtifact(fileName, Array.Empty<string>(), false,
                [$"Donnees incompatibles avec le format {fileName} : {exception.Message}"]);
        }
    }
}
