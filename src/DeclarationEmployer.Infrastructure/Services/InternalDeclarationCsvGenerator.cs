using System.Globalization;
using System.Text;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class InternalDeclarationCsvGenerator : IInternalDeclarationCsvGenerator
{
    private static readonly string[] Header =
    [
        "DeclarationId",
        "ClientCode",
        "ClientName",
        "FiscalYear",
        "LineId",
        "BeneficiaryIdentifierType",
        "BeneficiaryIdentifier",
        "BeneficiaryName",
        "OperationType",
        "FiscalCategory",
        "GrossAmount",
        "TaxableAmount",
        "Rate",
        "WithheldAmount",
        "PaymentDate",
        "DocumentReference",
        "Notes"
    ];

    public string Generate(InternalDeclarationCsvDocument document)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(';', Header));

        foreach (var line in document.Lines.Where(x => x.Status != Domain.Declarations.DeclarationLineStatus.Excluded))
        {
            builder.AppendLine(string.Join(
                ';',
                Escape(document.DeclarationId.ToString()),
                Escape(document.ClientCode),
                Escape(document.ClientName),
                Escape(document.FiscalYear.ToString(CultureInfo.InvariantCulture)),
                Escape(line.LineId.ToString()),
                Escape(line.BeneficiaryIdentifierType?.ToString()),
                Escape(line.BeneficiaryIdentifier),
                Escape(line.BeneficiaryName),
                Escape(line.OperationType),
                Escape(line.FiscalCategory),
                Escape(line.GrossAmount.ToString(CultureInfo.InvariantCulture)),
                Escape(line.TaxableAmount.ToString(CultureInfo.InvariantCulture)),
                Escape(line.Rate.ToString(CultureInfo.InvariantCulture)),
                Escape(line.WithheldAmount.ToString(CultureInfo.InvariantCulture)),
                Escape(line.PaymentDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Escape(line.DocumentReference),
                Escape(line.Notes)));
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        var escaped = text.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
