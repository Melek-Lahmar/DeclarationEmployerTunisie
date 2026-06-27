namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class OfficialAnnex2Generator
{
    public EmpccaGenerationArtifact Generate(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA2Record> records)
    {
        var issues = OfficialAnnexGeneratorSupport.CommonBlockingIssues(declarant).ToList();
        if (records.Count == 0) issues.Add("A2 doit contenir au moins un beneficiaire.");
        if (records.Any(x => x.Detail.AmountType is < 0 or > 6)) issues.Add("Le type montant A2 doit etre compris entre 0 et 6.");
        var lines = new List<string> { OfficialAnnexGeneratorSupport.Header(declarant, 2, records.Count) };
        lines.AddRange(records.Select(x => BuildLine(declarant, x)));
        lines.Add(BuildTotal(declarant, records));
        OfficialAnnexGeneratorSupport.ValidateRecords(lines);
        return new EmpccaGenerationArtifact("ANXEMP_2_25_1", lines, false, issues);
    }

    private static string BuildLine(EmpccaDeclarant declarant, EmpccaAnnexA2Record value)
    {
        var d = value.Detail;
        return string.Concat(OfficialAnnexGeneratorSupport.BeneficiaryPrefix(declarant, 2, value.OrderNumber, value.Beneficiary),
            FixedWidthFormatter.FormatInteger(d.AmountType, 1),
            OfficialAnnexGeneratorSupport.Amount(d.GrossProfessionalAmount), OfficialAnnexGeneratorSupport.Amount(d.RealRegimeFeesAmount),
            OfficialAnnexGeneratorSupport.Amount(d.BoardAndSecuritiesAmount), OfficialAnnexGeneratorSupport.Amount(d.OccasionalWorkAmount),
            OfficialAnnexGeneratorSupport.Amount(d.RealEstateCapitalGainAmount), OfficialAnnexGeneratorSupport.Amount(d.HotelRentAmount),
            OfficialAnnexGeneratorSupport.Amount(d.ArtistRemunerationAmount), OfficialAnnexGeneratorSupport.Amount(d.PublicSectorVatWithheldAmount),
            OfficialAnnexGeneratorSupport.Amount(d.WithheldAmount), OfficialAnnexGeneratorSupport.Amount(d.NetPaidAmount),
            FixedWidthFormatter.FormatAlpha(null, 10));
    }

    private static string BuildTotal(EmpccaDeclarant declarant, IReadOnlyList<EmpccaAnnexA2Record> values) => string.Concat(
        OfficialAnnexGeneratorSupport.TotalPrefix(declarant, 2, 221),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.GrossProfessionalAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.RealRegimeFeesAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.BoardAndSecuritiesAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.OccasionalWorkAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.RealEstateCapitalGainAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.HotelRentAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.ArtistRemunerationAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.PublicSectorVatWithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.WithheldAmount)),
        OfficialAnnexGeneratorSupport.Amount(values.Sum(x => x.Detail.NetPaidAmount)),
        FixedWidthFormatter.FormatAlpha(null, 10));
}
