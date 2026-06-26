namespace DeclarationEmployer.Contracts.Fiscal;

public sealed class FiscalReadinessDto
{
    public int Year { get; set; }

    public string RuleSetCode { get; set; } = string.Empty;

    public bool HasActiveRuleSet { get; set; }

    public bool IsOfficialGenerationEnabled { get; set; }

    public string Message { get; set; } = string.Empty;

    public int AnnexesCount { get; set; }

    public int ConfirmedAnnexesCount { get; set; }

    public int FieldsCount { get; set; }

    public int ConfirmedFieldsCount { get; set; }
}
