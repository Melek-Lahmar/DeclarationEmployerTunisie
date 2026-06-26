namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class ZeroRateWarningRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.Rate == 0)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Warning,
                "LINE_RATE_ZERO",
                "Le taux est egal a zero. Verifiez si cette situation est normale.",
                line))
            .ToList();
    }
}
