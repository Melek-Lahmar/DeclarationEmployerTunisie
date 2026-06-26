namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class RateMustBeBetweenZeroAndHundredRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.Rate < 0 || line.Rate > 100)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_RATE_INVALID",
                "Le taux doit etre compris entre 0 et 100.",
                line))
            .ToList();
    }
}
