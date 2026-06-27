namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class FixedWidthRecordDefinition
{
    public FixedWidthRecordDefinition(
        string recordName,
        string recordCode,
        int totalLength,
        IEnumerable<FixedWidthFieldDefinition> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordCode);

        if (totalLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalLength), "La longueur totale doit etre positive.");
        }

        RecordName = recordName;
        RecordCode = recordCode;
        TotalLength = totalLength;
        Fields = fields?.OrderBy(x => x.StartPosition).ToArray()
            ?? throw new ArgumentNullException(nameof(fields));

        Validate();
    }

    public string RecordName { get; }

    public string RecordCode { get; }

    public int TotalLength { get; }

    public IReadOnlyList<FixedWidthFieldDefinition> Fields { get; }

    private void Validate()
    {
        if (Fields.Count == 0)
        {
            throw new ArgumentException("Un enregistrement doit definir au moins un champ.", nameof(Fields));
        }

        var expectedStart = 1;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in Fields)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(field.Name);

            if (!names.Add(field.Name))
            {
                throw new ArgumentException($"Le champ '{field.Name}' est defini plusieurs fois.", nameof(Fields));
            }

            if (field.StartPosition != expectedStart)
            {
                throw new ArgumentException(
                    $"Le champ '{field.Name}' doit commencer a la position {expectedStart}, pas {field.StartPosition}.",
                    nameof(Fields));
            }

            if (field.EndPosition < field.StartPosition)
            {
                throw new ArgumentException($"Les positions du champ '{field.Name}' sont invalides.", nameof(Fields));
            }

            expectedStart = field.EndPosition + 1;
        }

        if (expectedStart - 1 != TotalLength)
        {
            throw new ArgumentException(
                $"Les champs couvrent {expectedStart - 1} caracteres au lieu de {TotalLength}.",
                nameof(Fields));
        }
    }
}
