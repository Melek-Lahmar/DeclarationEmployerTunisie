using System.Globalization;

namespace DeclarationEmployer.FiscalEngine.Empcca;

public static class FixedWidthFormatter
{
    public static string FormatAlpha(string? value, int length)
    {
        ValidateLength(length);
        value ??= string.Empty;
        EnsureAscii(value);

        if (value.Length > length)
        {
            throw new ArgumentException($"La valeur alphanumerique depasse {length} caracteres.", nameof(value));
        }

        return value.PadRight(length, ' ');
    }

    public static string FormatNumeric(string? value, int length)
    {
        ValidateLength(length);
        value ??= string.Empty;

        if (value.Length > 0 && value.Any(character => character is < '0' or > '9'))
        {
            throw new ArgumentException("Une zone numerique EMPCCA ne peut contenir que des chiffres.", nameof(value));
        }

        if (value.Length > length)
        {
            throw new ArgumentException($"La valeur numerique depasse {length} caracteres.", nameof(value));
        }

        return value.PadLeft(length, '0');
    }

    public static string FormatInteger(int value, int length)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Une valeur EMPCCA ne peut pas etre negative.");
        }

        return FormatNumeric(value.ToString(CultureInfo.InvariantCulture), length);
    }

    public static string FormatAmountInMillimes(decimal amount, int length)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Un montant EMPCCA ne peut pas etre negatif.");
        }

        var millimes = amount * 1000m;
        if (millimes != decimal.Truncate(millimes))
        {
            throw new ArgumentException("Le montant contient une fraction de millime.", nameof(amount));
        }

        return FormatNumeric(millimes.ToString("0", CultureInfo.InvariantCulture), length);
    }

    public static string FormatRate(decimal ratePercent, int length = 5)
    {
        if (ratePercent is < 0m or > 999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(ratePercent), "Le taux doit etre compris entre 0 et 999,99.");
        }

        var encodedRate = ratePercent * 100m;
        if (encodedRate != decimal.Truncate(encodedRate))
        {
            throw new ArgumentException("Le taux EMPCCA accepte au maximum deux decimales.", nameof(ratePercent));
        }

        return FormatNumeric(encodedRate.ToString("0", CultureInfo.InvariantCulture), length);
    }

    public static string FormatDate(DateOnly date) => date.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

    public static string FormatDate(DateTime date) => date.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

    public static void EnsureAscii(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] is < (char)32 or > (char)126)
            {
                throw new ArgumentException(
                    $"Le caractere a la position {index + 1} n'est pas un caractere ASCII imprimable.",
                    nameof(value));
            }
        }
    }

    public static void EnsureExactLength(string value, int expectedLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateLength(expectedLength);

        if (value.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Longueur invalide : {value.Length} caracteres au lieu de {expectedLength}.",
                nameof(value));
        }
    }

    private static void ValidateLength(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "La longueur doit etre positive.");
        }
    }
}
