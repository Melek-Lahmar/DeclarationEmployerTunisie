namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex7Generator
{
    private static readonly HashSet<int> AllowedTypes = [1, 2, 3, 4, 5, 6, 7, 8, 15, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29];

    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA7Record> records)
    {
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A7 doit contenir au moins un beneficiaire.");
        if (records.Any(x => !AllowedTypes.Contains(x.Detail.PaidAmountType))) issues.Add("Un type montant paye A7 est invalide.");
        if (records.Any(x => x.Detail.PaidAmountType == 29))
            issues.Add("Le type A7 29 figure dans le tableau page 65 mais contredit la mention '1 jusqu'a 28' de A712.");
        issues.Add("ANXFIN07 laisse la position 240 non decrite; elle est techniquement initialisee a zero.");
        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 7, records.Count) };
        lines.AddRange(records.Select(x => BuildLine(declarant, x)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_7_25_1", lines, false, issues);
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA7Record value) => string.Concat(
        OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 7, value.OrderNumber, value.Beneficiary),
        FixedWidthFormatter.FormatInteger(value.Detail.PaidAmountType, 2),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.PaidAmount),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.WithheldAmount),
        OfficialAnnexGeneratorSupport.Amount(value.Detail.NetPaidAmount),
        FixedWidthFormatter.FormatAlpha(null, 114));

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA7Record> values) => string.Concat(
        OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 7, 221),
        "0",
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PaidAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.WithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NetPaidAmount)),
        FixedWidthFormatter.FormatAlpha(null, 114));
}
