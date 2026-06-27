namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex6Generator
{
    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA6Record> records)
    {
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A6 doit contenir au moins un beneficiaire.");
        if (records.Any(x => x.Detail.RebateType is < 0 or > 2)) issues.Add("Le type ristourne A6 doit etre 0, 1 ou 2.");
        if (records.Any(x => x.Detail.RebateType == 0 && x.Detail.RebateAmount != 0)) issues.Add("Une ristourne non nulle exige le type 1 ou 2.");
        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 6, records.Count) };
        lines.AddRange(records.Select(x => BuildLine(declarant, x)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_6_25_1", lines, false, issues);
    }

    private static IEnumerable<decimal> Amounts(DeclarationEmployer.Domain.Declarations.Empcca.AnnexA6Detail d)
    {
        yield return d.RebateAmount; yield return d.FlatRegimeSalesAmount; yield return d.FlatRegimeSalesAdvanceAmount;
        yield return d.GamblingIncomeAmount; yield return d.GamblingWithheldAmount; yield return d.DistributionNetworkSalesAmount;
        yield return d.DistributionNetworkWithheldAmount; yield return d.CashCollectionsAmount; yield return d.AlcoholSalesAmount;
        yield return d.AlcoholSalesAdvanceAmount;
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA6Record value) => string.Concat(
        OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 6, value.OrderNumber, value.Beneficiary),
        FixedWidthFormatter.FormatInteger(value.Detail.RebateType, 1),
        string.Concat(Amounts(value.Detail).Select(OfficialAnnexGeneratorSupport.Amount)),
        FixedWidthFormatter.FormatAlpha(null, 10));

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA6Record> values)
    {
        var totals = Enumerable.Range(0, 10).Select(index => values.Sum(x => Amounts(x.Detail).ElementAt(index)));
        return string.Concat(OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 6, 221),
            string.Concat(totals.Select(OfficialAnnexGeneratorSupport.Amount)), FixedWidthFormatter.FormatAlpha(null, 10));
    }
}
