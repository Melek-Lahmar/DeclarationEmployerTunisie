namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA3Detail
{
    public Guid LineId { get; set; }
    public decimal SavingsAccountInterest { get; set; }
    public decimal OtherMovableCapitalIncome { get; set; }
    public decimal NonEstablishedBankLoanInterest { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
    public DeclarationLine? Line { get; set; }
}
