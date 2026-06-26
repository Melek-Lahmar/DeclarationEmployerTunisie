namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class PaymentDateMustBeInsideFiscalYearRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.PaymentDate.HasValue &&
                           line.PaymentDate.Value.Year != context.FiscalYear)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_PAYMENT_DATE_OUT_OF_YEAR",
                "La date de paiement doit appartenir a l'exercice fiscal.",
                line))
            .ToList();
    }
}
