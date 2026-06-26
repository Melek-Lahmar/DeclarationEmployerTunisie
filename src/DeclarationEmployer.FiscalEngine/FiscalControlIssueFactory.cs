namespace DeclarationEmployer.FiscalEngine;

internal static class FiscalControlIssueFactory
{
    internal static FiscalControlIssue Create(
        FiscalControlSeverity severity,
        string code,
        string message,
        FiscalControlLine line)
    {
        return new FiscalControlIssue
        {
            Severity = severity,
            Code = code,
            Message = message,
            EntityName = "DeclarationLine",
            EntityId = line.LineId.ToString()
        };
    }
}
