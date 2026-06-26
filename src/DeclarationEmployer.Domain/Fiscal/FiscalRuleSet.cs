namespace DeclarationEmployer.Domain.Fiscal;

public sealed class FiscalRuleSet
{
    public Guid Id { get; set; }

    public int Year { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string? SourceReference { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<AnnexDefinition> Annexes { get; set; } = new List<AnnexDefinition>();

    public ICollection<FiscalRateDefinition> Rates { get; set; } = new List<FiscalRateDefinition>();
}
