namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex3Generator
{
    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA3Record> records)
    {
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A3 doit contenir au moins un beneficiaire.");
        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 3, records.Count) };
        lines.AddRange(records.Select(x => BuildLine(declarant, x)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_3_25_1", lines, false, issues);
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA3Record value) => string.Concat(
        OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 3, value.OrderNumber, value.Beneficiary),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.SavingsAccountInterest),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.OtherMovableCapitalIncome),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.NonEstablishedBankLoanInterest),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.WithheldAmount),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.NetPaidAmount),
        FixedWidthFormatter.FormatAlpha(null, 86));

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA3Record> values) => string.Concat(
        OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 3, 220),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.SavingsAccountInterest)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.OtherMovableCapitalIncome)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NonEstablishedBankLoanInterest)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.WithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NetPaidAmount)),
        FixedWidthFormatter.FormatAlpha(null, 86));
}
