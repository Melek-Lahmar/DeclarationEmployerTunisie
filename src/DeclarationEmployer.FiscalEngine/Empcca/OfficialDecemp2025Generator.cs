namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed record EmpccaDecempRecordValue(decimal Basis, decimal RatePercent, decimal WithheldAmount);

public sealed class OfficialDecemp2025Generator
{
    public EmpccaGenerationArtifact Generate(
        EmpccaDeclarant declarant,
        IReadOnlySet<int> depositedAnnexes,
        IReadOnlyDictionary<int, EmpccaDecempRecordValue> values)
    {
        ArgumentNullException.ThrowIfNull(depositedAnnexes);
        ArgumentNullException.ThrowIfNull(values);

        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        issues.Add("DECEMP00 contient une contradiction de position dans le PDF (D000/D001); la disposition 1-3 puis 4-15 est une inference.");
        issues.Add("Les dispositions individuelles DECEMP01-49 doivent etre confirmees avant activation officielle.");

        if (depositedAnnexes.Any(x => x is < 1 or > 7))
            issues.Add("Les numeros d'annexe presents doivent etre compris entre 1 et 7.");
        if (values.Keys.Any(x => x is < 1 or > 49))
            issues.Add("Les valeurs DECEMP doivent cibler les enregistrements 1 a 49.");

        var lines = new List<string>(51) { BuildHeader(declarant, depositedAnnexes) };
        for (var recordNumber = 1; recordNumber <= 49; recordNumber++)
        {
            var value = values.GetValueOrDefault(recordNumber) ?? new EmpccaDecempRecordValue(0, 0, 0);
            lines.Add(BuildSummaryRecord(recordNumber, value));
        }

        lines.Add(BuildFooter(values.Values.Sum(x => x.WithheldAmount)));

        foreach (var line in lines)
        {
            FixedWidthFormatter.EnsureAscii(line);
            FixedWidthFormatter.EnsureExactLength(line, 38);
        }

        return new EmpccaGenerationArtifact("DECEMP_25", lines, false, issues);
    }

    private static string BuildHeader(EmpccaDeclarant value, IReadOnlySet<int> depositedAnnexes)
    {
        var identifier = string.Concat(
            FixedWidthFormatter.FormatNumeric(value.FiscalNumber, 7),
            FixedWidthFormatter.FormatAlpha(value.FiscalKey, 1),
            FixedWidthFormatter.FormatAlpha(value.Category, 1),
            FixedWidthFormatter.FormatNumeric(value.SecondaryEstablishment, 3));
        var presence = string.Concat(Enumerable.Range(1, 7).Select(number => depositedAnnexes.Contains(number) ? "0" : "1"));
        return string.Concat("000", identifier, FixedWidthFormatter.FormatInteger(value.Year, 4), presence,
            FixedWidthFormatter.FormatAlpha(null, 12));
    }

    private static string BuildSummaryRecord(int recordNumber, EmpccaDecempRecordValue value) => string.Concat(
        FixedWidthFormatter.FormatInteger(recordNumber, 3),
        FixedWidthFormatter.FormatAmountInMillimes(value.Basis, 15),
        FixedWidthFormatter.FormatRate(value.RatePercent),
        FixedWidthFormatter.FormatAmountInMillimes(value.WithheldAmount, 15));

    private static string BuildFooter(decimal totalWithheld) => string.Concat(
        "999",
        FixedWidthFormatter.FormatAlpha(null, 20),
        FixedWidthFormatter.FormatAmountInMillimes(totalWithheld, 15));
}
