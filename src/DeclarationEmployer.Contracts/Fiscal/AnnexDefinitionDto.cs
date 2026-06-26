namespace DeclarationEmployer.Contracts.Fiscal;

public sealed class AnnexDefinitionDto
{
    public Guid Id { get; set; }

    public Guid RuleSetId { get; set; }

    public int Year { get; set; }

    public string RuleSetCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsOfficialMappingConfirmed { get; set; }

    public string? Notes { get; set; }

    public int FieldsCount { get; set; }
}
