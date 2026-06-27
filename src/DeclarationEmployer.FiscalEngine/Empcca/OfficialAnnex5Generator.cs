namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex5Generator
{
    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA5Record> records)
    {
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A5 doit contenir au moins un beneficiaire.");
        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 5, records.Count) };
        lines.AddRange(records.Select(x => BuildLine(declarant, x)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_5_25_1", lines, false, issues);
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA5Record value)
    {
        var d = value.Detail;
        return string.Concat(OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 5, value.OrderNumber, value.Beneficiary),
            OfficialAnnexGeneratorSupport.Amount(d.PurchasesFromTenPercentCompanies),
            OfficialAnnexGeneratorSupport.Amount(d.PurchasesFromFifteenPercentCompanies),
            OfficialAnnexGeneratorSupport.Amount(d.PurchasesFromTwoThirdsDeductionBusinesses),
            OfficialAnnexGeneratorSupport.Amount(d.PurchasesFromOtherBusinesses),
            OfficialAnnexGeneratorSupport.Amount(d.VatWithheldAmount),
            OfficialAnnexGeneratorSupport.Amount(d.DeliveryPlatformThreePercentWithheldAmount),
            OfficialAnnexGeneratorSupport.Amount(d.WithheldAmount), OfficialAnnexGeneratorSupport.Amount(d.NetPaidAmount),
            FixedWidthFormatter.FormatAlpha(null, 41));
    }

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA5Record> values) => string.Concat(
        OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 5, 220),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PurchasesFromTenPercentCompanies)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PurchasesFromFifteenPercentCompanies)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PurchasesFromTwoThirdsDeductionBusinesses)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PurchasesFromOtherBusinesses)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.VatWithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.DeliveryPlatformThreePercentWithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.WithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NetPaidAmount)),
        FixedWidthFormatter.FormatAlpha(null, 41));
}
