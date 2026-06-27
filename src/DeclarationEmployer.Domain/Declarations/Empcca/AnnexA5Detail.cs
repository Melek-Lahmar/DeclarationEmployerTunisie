namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA5Detail
{
    public Guid LineId { get; set; }
    public decimal PurchasesFromTenPercentCompanies { get; set; }
    public decimal PurchasesFromFifteenPercentCompanies { get; set; }
    public decimal PurchasesFromTwoThirdsDeductionBusinesses { get; set; }
    public decimal PurchasesFromOtherBusinesses { get; set; }
    public decimal VatWithheldAmount { get; set; }
    public decimal DeliveryPlatformThreePercentWithheldAmount { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
    public DeclarationLine? Line { get; set; }
}
