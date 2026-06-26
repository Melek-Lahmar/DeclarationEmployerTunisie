namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class ZeroTaxableWithWithheldAmountWarningRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.TaxableAmount == 0 && line.WithheldAmount > 0)
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Warning,
                "LINE_ZERO_TAXABLE_WITH_WITHHELD",
                "Une retenue existe alors que le montant imposable est nul.",
                line))
            .ToList();
    }
}
