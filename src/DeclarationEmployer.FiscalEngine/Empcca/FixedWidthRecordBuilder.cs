using System.Text;

namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class FixedWidthRecordBuilder
{
    public string Build(
        FixedWidthRecordDefinition definition,
        IReadOnlyDictionary<string, string?> values)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(values);

        var builder = new StringBuilder(definition.TotalLength);

        foreach (var field in definition.Fields)
        {
            values.TryGetValue(field.Name, out var suppliedValue);
            var value = suppliedValue ?? field.DefaultValue;

            if (field.Required && string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(
                    $"Le champ obligatoire '{field.Name}' de {definition.RecordCode} est absent.",
                    nameof(values));
            }

            builder.Append(field.Type == FixedWidthFieldType.Numeric
                ? FixedWidthFormatter.FormatNumeric(value, field.Length)
                : FixedWidthFormatter.FormatAlpha(value, field.Length));
        }

        var record = builder.ToString();
        FixedWidthFormatter.EnsureAscii(record);
        FixedWidthFormatter.EnsureExactLength(record, definition.TotalLength);
        return record;
    }
}
