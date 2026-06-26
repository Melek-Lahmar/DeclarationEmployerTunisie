namespace DeclarationEmployer.FiscalEngine.Rules;

public sealed class BeneficiaryRequiredRule : IFiscalControlRule
{
    public IReadOnlyList<FiscalControlIssue> Evaluate(FiscalControlContext context)
    {
        return context.Lines
            .Where(line => line.BeneficiaryId is null ||
                           string.IsNullOrWhiteSpace(line.BeneficiaryIdentifier) ||
                           string.IsNullOrWhiteSpace(line.BeneficiaryName))
            .Select(line => FiscalControlIssueFactory.Create(
                FiscalControlSeverity.Blocking,
                "LINE_BENEFICIARY_REQUIRED",
                "Le beneficiaire est obligatoire pour cette ligne.",
                line))
            .ToList();
    }
}
