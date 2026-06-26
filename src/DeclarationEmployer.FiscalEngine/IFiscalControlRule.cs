namespace DeclarationEmployer.FiscalEngine;

public interface IFiscalControlRule
{
    IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context);
}
