namespace DeclarationEmployer.FiscalEngine.Empcca;

public sealed class FixedWidthFieldDefinition
{
    public required string Name { get; init; }

    public required int StartPosition { get; init; }

    public required int EndPosition { get; init; }

    public int Length => EndPosition - StartPosition + 1;

    public required FixedWidthFieldType Type { get; init; }

    public bool Required { get; init; }

    public string? DefaultValue { get; init; }

    public string? Description { get; init; }
}
