namespace DeclarationEmployer.Contracts.Fiscal;

public sealed class FiscalRateDefinitionDto
{
    public Guid Id { get; set; }

    public Guid RuleSetId { get; set; }

    public int Year { get; set; }

    public string RuleSetCode { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public decimal Rate { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string? SourceReference { get; set; }

    public bool IsConfirmed { get; set; }

    public string? Notes { get; set; }
}
