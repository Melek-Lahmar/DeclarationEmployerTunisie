namespace DeclarationEmployer.Domain.Fiscal;

public sealed class FiscalRateDefinition
{
    public Guid Id { get; set; }

    public Guid RuleSetId { get; set; }

    public FiscalRuleSet? RuleSet { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public decimal Rate { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? SourceReference { get; set; }

    public bool IsConfirmed { get; set; }

    public string? Notes { get; set; }
}
