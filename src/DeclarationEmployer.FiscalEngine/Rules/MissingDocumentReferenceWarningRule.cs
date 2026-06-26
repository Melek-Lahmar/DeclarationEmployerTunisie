namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class MissingDocumentReferenceWarningRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => string.IsNullOrWhiteSpace(line.DocumentReference))
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Warning,
                "LINE_DOCUMENT_REFERENCE_MISSING",
                "La reference du document est absente.",
                line))
            .ToList();
    }
}
