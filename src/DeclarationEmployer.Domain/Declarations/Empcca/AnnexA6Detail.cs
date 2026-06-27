namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA6Detail
{
    public Guid LineId { get; set; }
    public int RebateType { get; set; }
    public decimal RebateAmount { get; set; }
    public decimal FlatRegimeSalesAmount { get; set; }
    public decimal FlatRegimeSalesAdvanceAmount { get; set; }
    public decimal GamblingIncomeAmount { get; set; }
    public decimal GamblingWithheldAmount { get; set; }
    public decimal DistributionNetworkSalesAmount { get; set; }
    public decimal DistributionNetworkWithheldAmount { get; set; }
    public decimal CashCollectionsAmount { get; set; }
    public decimal AlcoholSalesAmount { get; set; }
    public decimal AlcoholSalesAdvanceAmount { get; set; }
    public DeclarationLine? Line { get; set; }
}
