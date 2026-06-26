using DeclarationEmployer.Domain.Fiscal;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class FiscalReferenceSeedService
{
    public const string OfficialMappingIncompleteMessage =
        "Génération officielle non activée : mapping EMPCCA 2025 incomplet ou non confirmé.";

    private static readonly Guid RuleSet2025Id = Guid.Parse("8b8d29ef-0f78-4a5a-9c0e-8fc7d95f2025");

    private readonly ApplicationDbContext _db;

    public FiscalReferenceSeedService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.FiscalRuleSets.AnyAsync(x => x.Year == 2025 && x.Code == "EMPCCA-2025-FOUNDATION", cancellationToken))
        {
            return;
        }

        var ruleSet = new FiscalRuleSet
        {
            Id = RuleSet2025Id,
            Year = 2025,
            Code = "EMPCCA-2025-FOUNDATION",
            Name = "Referentiel fiscal EMPCCA 2025 foundation",
            SourceName = "Structure interne non officielle",
            SourceReference = "Cahier des charges officiel EMPCCA 2025 non fourni dans le repository.",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var annexes = new[]
        {
            CreateAnnex("A1", "Annexe I - Traitements, salaires, pensions et rentes viageres", 1),
            CreateAnnex("A2", "Annexe II - Foundation", 2),
            CreateAnnex("A3", "Annexe III - Foundation", 3),
            CreateAnnex("A4", "Annexe IV - Foundation", 4),
            CreateAnnex("A5", "Annexe V - Foundation", 5),
            CreateAnnex("A6", "Annexe VI - Foundation", 6),
            CreateAnnex("A7", "Annexe VII - Foundation", 7)
        };

        foreach (var annex in annexes)
        {
            ruleSet.Annexes.Add(annex);
        }

        AddFoundationFields(annexes[0], includePeriodFields: true);

        foreach (var annex in annexes.Skip(1))
        {
            AddFoundationFields(annex, includePeriodFields: false);
        }

        ruleSet.Rates.Add(new FiscalRateDefinition
        {
            Id = Guid.NewGuid(),
            Code = "UNCONFIRMED",
            Label = "Taux non confirme",
            Rate = 0,
            EffectiveFrom = new DateOnly(2025, 1, 1),
            IsConfirmed = false,
            Notes = OfficialMappingIncompleteMessage
        });

        _db.FiscalRuleSets.Add(ruleSet);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static AnnexDefinition CreateAnnex(string code, string name, int sortOrder)
    {
        return new AnnexDefinition
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = "Definition foundation parametrable, non utilisable comme mapping officiel.",
            SortOrder = sortOrder,
            IsActive = true,
            IsOfficialMappingConfirmed = false,
            Notes = OfficialMappingIncompleteMessage
        };
    }

    private static void AddFoundationFields(AnnexDefinition annex, bool includePeriodFields)
    {
        AddField(annex, "OrderNumber", "Numero d'ordre", FiscalDataType.Number, true);
        AddField(annex, "BeneficiaryIdentifier", "Identifiant beneficiaire", FiscalDataType.Text, true);
        AddField(annex, "BeneficiaryName", "Nom ou raison sociale beneficiaire", FiscalDataType.Text, true);
        AddField(annex, "OperationType", "Type operation", FiscalDataType.Text, true);
        AddField(annex, "GrossAmount", "Montant brut", FiscalDataType.Amount, true);
        AddField(annex, "WithholdingAmount", "Retenue", FiscalDataType.Amount, true);
        AddField(annex, "NetAmount", "Net paye", FiscalDataType.Amount, false);

        if (includePeriodFields)
        {
            AddField(annex, "PeriodStart", "Date debut periode", FiscalDataType.Date, false);
            AddField(annex, "PeriodEnd", "Date fin periode", FiscalDataType.Date, false);
        }
    }

    private static void AddField(
        AnnexDefinition annex,
        string code,
        string label,
        FiscalDataType dataType,
        bool isRequired)
    {
        annex.Fields.Add(new FiscalFieldDefinition
        {
            Id = Guid.NewGuid(),
            Code = code,
            Label = label,
            DataType = dataType,
            IsRequired = isRequired,
            PaddingType = FiscalPaddingType.None,
            IsConfirmed = false,
            Notes = OfficialMappingIncompleteMessage
        });
    }
}
