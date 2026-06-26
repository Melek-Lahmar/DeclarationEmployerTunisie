namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class OperationTypeRequiredRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => string.IsNullOrWhiteSpace(line.OperationType))
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_OPERATION_TYPE_REQUIRED",
                "Le type d'operation est obligatoire.",
                line))
            .ToList();
    }
}
