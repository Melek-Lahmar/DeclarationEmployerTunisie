namespace DeclarationEmployer.Domain.Fiscal;

public sealed class FiscalFieldDefinition
{
    public Guid Id { get; set; }

    public Guid AnnexDefinitionId { get; set; }

    public AnnexDefinition? AnnexDefinition { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public FiscalDataType DataType { get; set; }

    public bool IsRequired { get; set; }

    public int? Length { get; set; }

    public int? PositionStart { get; set; }

    public int? PositionEnd { get; set; }

    public FiscalPaddingType PaddingType { get; set; }

    public string? DefaultValue { get; set; }

    public string? SourceReference { get; set; }

    public bool IsConfirmed { get; set; }

    public string? Notes { get; set; }
}
