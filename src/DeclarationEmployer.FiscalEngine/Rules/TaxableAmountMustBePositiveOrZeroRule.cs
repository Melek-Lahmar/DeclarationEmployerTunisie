namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class TaxableAmountMustBePositiveOrZeroRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.TaxableAmount < 0)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_TAXABLE_AMOUNT_NEGATIVE",
                "Le montant imposable ne peut pas etre negatif.",
                line))
            .ToList();
    }
}
