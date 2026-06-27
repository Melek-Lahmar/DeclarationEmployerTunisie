namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class EmpccaDetailedLineDto<TDetails>
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public TDetails Details { get; set; } = default!;
}

public sealed class CreateEmpccaAnnexA3LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public decimal SavingsAccountInterest { get; set; }
    public decimal OtherMovableCapitalIncome { get; set; }
    public decimal NonEstablishedBankLoanInterest { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
}

public sealed class CreateEmpccaAnnexA4LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
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
}

public sealed class CreateEmpccaAnnexA6LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
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
}

public sealed class CreateEmpccaAnnexA7LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public int PaidAmountType { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
}
