namespace DeclarationEmployer.Domain.Fiscal;

public sealed class AnnexDefinition
{
    public Guid Id { get; set; }

    public Guid RuleSetId { get; set; }

    public FiscalRuleSet? RuleSet { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsOfficialMappingConfirmed { get; set; }

    public string? Notes { get; set; }

    public ICollection<FiscalFieldDefinition> Fields { get; set; } = new List<FiscalFieldDefinition>();
}
