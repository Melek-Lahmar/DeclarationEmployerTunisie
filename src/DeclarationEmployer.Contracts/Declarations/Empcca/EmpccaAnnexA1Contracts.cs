namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class CreateEmpccaAnnexA1LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public int FamilySituation { get; set; }
    public int DependentChildrenCount { get; set; }
    public DateOnly WorkPeriodStart { get; set; }
    public DateOnly WorkPeriodEnd { get; set; }
    public int WorkPeriodDays { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal BenefitsInKind { get; set; }
    public decimal GrossTaxableIncome { get; set; }
    public decimal ReinvestedIncome { get; set; }
    public decimal CommonRegimeWithheldAmount { get; set; }
    public decimal ForeignEmployeeWithheldAmount { get; set; }
    public decimal SocialSolidarityContribution { get; set; }
    public decimal NetPaidAmount { get; set; }
}

public sealed class EmpccaAnnexA1LineDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public CreateEmpccaAnnexA1LineRequest Details { get; set; } = new();
}

public sealed class EmpccaAnnexA1SummaryDto
{
    public int LineCount { get; set; }
    public decimal TaxableIncomeTotal { get; set; }
    public decimal GrossTaxableIncomeTotal { get; set; }
    public decimal CommonRegimeWithheldTotal { get; set; }
    public decimal ForeignEmployeeWithheldTotal { get; set; }
    public decimal SocialSolidarityContributionTotal { get; set; }
    public decimal NetPaidTotal { get; set; }
}
