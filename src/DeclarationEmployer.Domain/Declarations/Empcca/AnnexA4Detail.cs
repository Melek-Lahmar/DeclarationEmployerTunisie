namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA4Detail
{
    public Guid LineId { get; set; }
    public int AmountType { get; set; }
    public decimal ProfessionalAmountRate { get; set; }
    public decimal ProfessionalAmount { get; set; }
    public decimal ConstructionWorkRate { get; set; }
    public decimal ConstructionWorkAmount { get; set; }
    public decimal RealEstateCapitalGainRate { get; set; }
    public decimal RealEstateCapitalGainAmount { get; set; }
    public decimal SecuritiesCapitalGainRate { get; set; }
    public decimal SecuritiesCapitalGainAmount { get; set; }
    public decimal SecuritiesIncomeRate { get; set; }
    public decimal SecuritiesIncomeAmount { get; set; }
    public decimal PrivilegedTaxRegimeAmount { get; set; }
    public decimal VatWithheldAmount { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
    public DeclarationLine? Line { get; set; }
}
