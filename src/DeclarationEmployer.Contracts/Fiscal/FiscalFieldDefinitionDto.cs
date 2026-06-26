namespace DeclarationEmployer.Contracts.Fiscal;

public sealed class FiscalFieldDefinitionDto
{
    public Guid Id { get; set; }

    public Guid AnnexDefinitionId { get; set; }

    public string AnnexCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public int? Length { get; set; }

    public int? PositionStart { get; set; }

    public int? PositionEnd { get; set; }

    public string PaddingType { get; set; } = string.Empty;

    public string? DefaultValue { get; set; }

    public string? SourceReference { get; set; }

    public bool IsConfirmed { get; set; }

    public string? Notes { get; set; }
}
