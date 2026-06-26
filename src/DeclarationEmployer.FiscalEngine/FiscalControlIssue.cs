namespace DeclarationEmployer.FiscalEngine;

public sealed class FiscalControlIssue
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public FiscalControlSeverity Severity { get; set; }

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }
}
