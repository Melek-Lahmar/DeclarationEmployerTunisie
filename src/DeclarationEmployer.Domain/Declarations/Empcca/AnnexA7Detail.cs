namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA7Detail
{
    public Guid LineId { get; set; }
    public int PaidAmountType { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
    public DeclarationLine? Line { get; set; }
}
