namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex4Generator
{
    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA4Record> records)
    {
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A4 doit contenir au moins un beneficiaire.");
        if (records.Any(x => x.Detail.AmountType is < 0 or > 9)) issues.Add("Le type montant A4 doit etre compris entre 0 et 9.");
        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 4, records.Count) };
        lines.AddRange(records.Select(x => BuildLine(declarant, x)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_4_25_1", lines, false, issues);
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA4Record value)
    {
        var d = value.Detail;
        return string.Concat(OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 4, value.OrderNumber, value.Beneficiary),
            FixedWidthFormatter.FormatInteger(d.AmountType, 1),
            FixedWidthFormatter.FormatRate(d.ProfessionalAmountRate), OfficialAnnexGeneratorSupport.Amount(d.ProfessionalAmount),
            FixedWidthFormatter.FormatRate(d.ConstructionWorkRate), OfficialAnnexGeneratorSupport.Amount(d.ConstructionWorkAmount),
            FixedWidthFormatter.FormatRate(d.RealEstateCapitalGainRate), OfficialAnnexGeneratorSupport.Amount(d.RealEstateCapitalGainAmount),
            FixedWidthFormatter.FormatRate(d.SecuritiesCapitalGainRate), OfficialAnnexGeneratorSupport.Amount(d.SecuritiesCapitalGainAmount),
            FixedWidthFormatter.FormatRate(d.SecuritiesIncomeRate), OfficialAnnexGeneratorSupport.Amount(d.SecuritiesIncomeAmount),
            OfficialAnnexGeneratorSupport.Amount(d.PrivilegedTaxRegimeAmount), OfficialAnnexGeneratorSupport.Amount(d.VatWithheldAmount),
            OfficialAnnexGeneratorSupport.Amount(d.WithheldAmount), OfficialAnnexGeneratorSupport.Amount(d.NetPaidAmount));
    }

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA4Record> values) => string.Concat(
        OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 4, 221),
        "00000", OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.ProfessionalAmount)),
        "00000", OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.ConstructionWorkAmount)),
        "00000", OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.RealEstateCapitalGainAmount)),
        "00000", OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.SecuritiesCapitalGainAmount)),
        "00000", OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.SecuritiesIncomeAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PrivilegedTaxRegimeAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.VatWithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.WithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NetPaidAmount)));
}
