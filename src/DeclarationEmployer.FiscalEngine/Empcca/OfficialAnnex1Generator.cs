namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex1Generator
{
    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA1Record> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A1 doit contenir au moins un beneficiaire.");
        if (records.Any(x => x.Beneficiary.IdentifierType != 2)) issues.Add("A1 accepte uniquement le type identifiant CIN (2).");
        if (records.Any(x => x.Beneficiary.Identifier.Length != 8 || !x.Beneficiary.Identifier.All(char.IsAsciiDigit)))
            issues.Add("Le CIN A1 est encode sur 8 chiffres selon le tableau ANXBEN01; cette interpretation reste a confirmer.");

        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 1, records.Count) };
        lines.AddRange(records.Select(record => BuildLine(declarant, record)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_1_25_1", lines, false, issues);
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA1Record value)
    {
        var d = value.Detail;
        return string.Concat(
            OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 1, value.OrderNumber, value.Beneficiary),
            FixedWidthFormatter.FormatInteger(d.FamilySituation, 1),
            FixedWidthFormatter.FormatInteger(d.DependentChildrenCount, 2),
            FixedWidthFormatter.FormatDate(d.WorkPeriodStart),
            FixedWidthFormatter.FormatDate(d.WorkPeriodEnd),
            FixedWidthFormatter.FormatInteger(d.WorkPeriodDays, 3),
            OfficialAnnexGeneratorSupport.Amount(d.TaxableIncome),
            OfficialAnnexGeneratorSupport.Amount(d.BenefitsInKind),
            OfficialAnnexGeneratorSupport.Amount(d.GrossTaxableIncome),
            OfficialAnnexGeneratorSupport.Amount(d.ReinvestedIncome),
            OfficialAnnexGeneratorSupport.Amount(d.CommonRegimeWithheldAmount),
            OfficialAnnexGeneratorSupport.Amount(d.ForeignEmployeeWithheldAmount),
            OfficialAnnexGeneratorSupport.Amount(d.SocialSolidarityContribution),
            OfficialAnnexGeneratorSupport.Amount(d.NetPaidAmount),
            FixedWidthFormatter.FormatAlpha(null, 19));
    }

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA1Record> values) => string.Concat(
        OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 1, 242),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.TaxableIncome)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.BenefitsInKind)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.GrossTaxableIncome)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.ReinvestedIncome)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.CommonRegimeWithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.ForeignEmployeeWithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.SocialSolidarityContribution)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NetPaidAmount)),
        FixedWidthFormatter.FormatAlpha(null, 19));
}
