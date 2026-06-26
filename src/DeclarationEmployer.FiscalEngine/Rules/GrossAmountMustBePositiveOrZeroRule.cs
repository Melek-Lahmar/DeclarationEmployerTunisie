namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class GrossAmountMustBePositiveOrZeroRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.GrossAmount < 0)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_GROSS_AMOUNT_NEGATIVE",
                "Le montant brut ne peut pas etre negatif.",
                line))
            .ToList();
    }
}
