namespace DeclarationEmployer.FiscalEngine.Empcca;

internal static class OfficialAnnexGeneratorSupport
{
    public static string Header(EmpccaDeclarant declarant, int annexNumber, int beneficiaryCount)
    {
        var value = string.Concat(
            $"E{annexNumber}", DeclarantIdentifier(declarant), Year(declarant), $"An{annexNumber}",
            FixedWidthFormatter.FormatInteger(declarant.ActCode, 1),
            FixedWidthFormatter.FormatInteger(beneficiaryCount, 6),
            FixedWidthFormatter.FormatAlpha(declarant.Name, 40),
            FixedWidthFormatter.FormatAlpha(declarant.Activity, 40),
            FixedWidthFormatter.FormatAlpha(declarant.City, 40),
            FixedWidthFormatter.FormatAlpha(declarant.Street, 72),
            FixedWidthFormatter.FormatNumeric(declarant.StreetNumber, 4),
            FixedWidthFormatter.FormatNumeric(declarant.PostalCode, 4),
            FixedWidthFormatter.FormatAlpha(null, 171));
        ValidateRecord(value, 399);
        return value;
    }

    public static string BeneficiaryPrefix(
        EmpccaDeclarant declarant,
        int annexNumber,
        int orderNumber,
        EmpccaBeneficiary beneficiary)
    {
        var value = string.Concat(
            $"L{annexNumber}", DeclarantIdentifier(declarant), Year(declarant),
            FixedWidthFormatter.FormatInteger(orderNumber, 6),
            FixedWidthFormatter.FormatInteger(beneficiary.IdentifierType, 1),
            FixedWidthFormatter.FormatAlpha(beneficiary.Identifier, 13),
            FixedWidthFormatter.FormatAlpha(beneficiary.Name, 40),
            FixedWidthFormatter.FormatAlpha(beneficiary.ActivityOrJob, 40),
            FixedWidthFormatter.FormatAlpha(beneficiary.Address, 120));
        ValidateRecord(value, 238);
        return value;
    }

    public static string TotalPrefix(EmpccaDeclarant declarant, int annexNumber, int reservedLength)
    {
        var value = string.Concat(
            $"T{annexNumber}", DeclarantIdentifier(declarant), Year(declarant),
            FixedWidthFormatter.FormatAlpha(null, reservedLength));
        ValidateRecord(value, 18 + reservedLength);
        return value;
    }

    public static string Amount(decimal value) => FixedWidthFormatter.FormatAmountInMillimes(value, 15);

    public static void ValidateRecords(IEnumerable<string> records)
    {
        foreach (var record in records)
        {
            ValidateRecord(record, 399);
        }
    }

    public static IReadOnlyList<string> CommonBlockingIssues(EmpccaDeclarant declarant)
    {
        var issues = new List<string>();
        if (declarant.FiscalNumber.Length != 7 || !declarant.FiscalNumber.All(char.IsAsciiDigit))
            issues.Add("Le matricule fiscal declarant doit contenir 7 chiffres.");
        if (declarant.FiscalKey.Length != 1) issues.Add("La cle fiscale declarant doit contenir un caractere.");
        if (declarant.Category.Length != 1 || declarant.Category.Equals("E", StringComparison.OrdinalIgnoreCase))
            issues.Add("La categorie declarant doit contenir un caractere different de E.");
        if (declarant.SecondaryEstablishment != "000") issues.Add("L'etablissement secondaire declarant doit etre 000.");
        if (declarant.Year != 2025) issues.Add("Le format EMPCCA analyse est limite a l'exercice 2025.");
        if (declarant.ActCode is < 0 or > 2) issues.Add("Le code acte doit etre 0, 1 ou 2.");
        return issues;
    }

    private static string DeclarantIdentifier(EmpccaDeclarant value) => string.Concat(
        FixedWidthFormatter.FormatNumeric(value.FiscalNumber, 7),
        FixedWidthFormatter.FormatAlpha(value.FiscalKey, 1),
        FixedWidthFormatter.FormatAlpha(value.Category, 1),
        FixedWidthFormatter.FormatNumeric(value.SecondaryEstablishment, 3));

    private static string Year(EmpccaDeclarant value) => FixedWidthFormatter.FormatNumeric(
        value.Year.ToString(System.Globalization.CultureInfo.InvariantCulture), 4);

    private static void ValidateRecord(string record, int length)
    {
        FixedWidthFormatter.EnsureAscii(record);
        FixedWidthFormatter.EnsureExactLength(record, length);
    }
}
