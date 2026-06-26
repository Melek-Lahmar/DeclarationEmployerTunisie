namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class MissingFiscalCategoryInfoRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => string.IsNullOrWhiteSpace(line.FiscalCategory))
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Info,
                "LINE_FISCAL_CATEGORY_MISSING",
                "La categorie fiscale n'est pas renseignee.",
                line))
            .ToList();
    }
}
