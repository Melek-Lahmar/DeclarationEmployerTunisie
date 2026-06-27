namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class CreateEmpccaAnnexA5LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public decimal PurchasesFromTenPercentCompanies { get; set; }
    public decimal PurchasesFromFifteenPercentCompanies { get; set; }
    public decimal PurchasesFromTwoThirdsDeductionBusinesses { get; set; }
    public decimal PurchasesFromOtherBusinesses { get; set; }
    public decimal VatWithheldAmount { get; set; }
    public decimal DeliveryPlatformThreePercentWithheldAmount { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
}

public sealed class EmpccaAnnexA5LineDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public CreateEmpccaAnnexA5LineRequest Details { get; set; } = new();
}

public sealed class EmpccaAnnexA5SummaryDto
{
    public int LineCount { get; set; }
    public decimal PurchasesTotal { get; set; }
    public decimal VatWithheldTotal { get; set; }
    public decimal DeliveryPlatformThreePercentWithheldTotal { get; set; }
    public decimal WithheldTotal { get; set; }
    public decimal NetPaidTotal { get; set; }
}
