namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class WithheldAmountMustBePositiveOrZeroRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.WithheldAmount < 0)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_WITHHELD_AMOUNT_NEGATIVE",
                "Le montant retenu ne peut pas etre negatif.",
                line))
            .ToList();
    }
}
