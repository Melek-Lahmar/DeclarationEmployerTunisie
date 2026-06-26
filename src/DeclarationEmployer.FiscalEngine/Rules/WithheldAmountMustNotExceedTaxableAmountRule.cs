namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class WithheldAmountMustNotExceedTaxableAmountRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.WithheldAmount > line.TaxableAmount)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_WITHHELD_EXCEEDS_TAXABLE",
                "La retenue ne peut pas depasser le montant imposable.",
                line))
            .ToList();
    }
}
