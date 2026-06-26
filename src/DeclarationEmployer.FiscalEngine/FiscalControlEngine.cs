namespace DeclarationEmployer.FiscalEngine;

public sealed class FiscalControlEngine : IFiscalControlEngine
{
    private readonly IReadOnlyList<IFiscalControlRule> _rules;

    public FiscalControlEngine(IEnumerable<IFiscalControlRule> rules)
    {
        _rules = rules.ToList();
    }

    public FiscalControlResult Run(FiscalControlContext context)
    {
        var issues = _rules
            .SelectMany(rule => rule.Evaluate(context))
            .ToList();

        return new FiscalControlResult
        {
            DeclarationId = context.DeclarationId,
            CheckedLinesCount = context.Lines.Count,
            BlockingIssuesCount = issues.Count(x => x.Severity == FiscalControlSeverity.Blocking),
            WarningIssuesCount = issues.Count(x => x.Severity == FiscalControlSeverity.Warning),
            InfoIssuesCount = issues.Count(x => x.Severity == FiscalControlSeverity.Info),
            Issues = issues
        };
    }
}
