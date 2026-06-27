namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA1Detail
{
    public Guid LineId { get; set; }
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
    public DeclarationLine? Line { get; set; }
}
