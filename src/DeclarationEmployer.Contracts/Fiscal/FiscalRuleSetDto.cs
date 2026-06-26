namespace DeclarationEmployer.Contracts.Fiscal;

public sealed class FiscalRuleSetDto
{
    public Guid Id { get; set; }

    public int Year { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string? SourceReference { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int AnnexesCount { get; set; }

    public int ConfirmedAnnexesCount { get; set; }
}
