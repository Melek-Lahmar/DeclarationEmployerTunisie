using System.Text;

namespace DeclarationEmployer.Import;

public static class ExcelImportColumnNames
{
    public const string IdentifierType = "IdentifierType";
    public const string Identifier = "Identifier";
    public const string BeneficiaryName = "BeneficiaryName";
    public const string OperationType = "OperationType";
    public const string FiscalCategory = "FiscalCategory";
    public const string GrossAmount = "GrossAmount";
    public const string TaxableAmount = "TaxableAmount";
    public const string Rate = "Rate";
    public const string WithheldAmount = "WithheldAmount";
    public const string PaymentDate = "PaymentDate";
    public const string DocumentReference = "DocumentReference";
    public const string Notes = "Notes";
    public const string Address = "Address";
    public const string Country = "Country";
    public const string IsResident = "IsResident";

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        [Normalize(IdentifierType)] = IdentifierType,
        [Normalize("TypeIdentifiant")] = IdentifierType,
        [Normalize(Identifier)] = Identifier,
        [Normalize("Identifiant")] = Identifier,
        [Normalize(BeneficiaryName)] = BeneficiaryName,
        [Normalize("NomBeneficiaire")] = BeneficiaryName,
        [Normalize(OperationType)] = OperationType,
        [Normalize("TypeOperation")] = OperationType,
        [Normalize(FiscalCategory)] = FiscalCategory,
        [Normalize("CategorieFiscale")] = FiscalCategory,
        [Normalize(GrossAmount)] = GrossAmount,
        [Normalize("MontantBrut")] = GrossAmount,
        [Normalize(TaxableAmount)] = TaxableAmount,
        [Normalize("MontantImposable")] = TaxableAmount,
        [Normalize(Rate)] = Rate,
        [Normalize("Taux")] = Rate,
        [Normalize(WithheldAmount)] = WithheldAmount,
        [Normalize("Retenue")] = WithheldAmount,
        [Normalize(PaymentDate)] = PaymentDate,
        [Normalize("DatePaiement")] = PaymentDate,
        [Normalize(DocumentReference)] = DocumentReference,
        [Normalize("ReferenceDocument")] = DocumentReference,
        [Normalize(Notes)] = Notes,
        [Normalize(Address)] = Address,
        [Normalize("Adresse")] = Address,
        [Normalize(Country)] = Country,
        [Normalize("Pays")] = Country,
        [Normalize(IsResident)] = IsResident,
        [Normalize("Resident")] = IsResident
    };

    public static readonly string[] RequiredColumns =
    [
        IdentifierType,
        Identifier,
        BeneficiaryName,
        OperationType,
        GrossAmount,
        TaxableAmount,
        Rate,
        WithheldAmount
    ];

    public static bool TryResolve(string? rawHeader, out string canonicalName)
    {
        if (!string.IsNullOrWhiteSpace(rawHeader) &&
            Aliases.TryGetValue(Normalize(rawHeader), out var resolvedName))
        {
            canonicalName = resolvedName;
            return true;
        }

        canonicalName = string.Empty;
        return false;
    }

    public static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (character is >= 'A' and <= 'Z' ||
                character is >= 'a' and <= 'z' ||
                character is >= '0' and <= '9')
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }
}
