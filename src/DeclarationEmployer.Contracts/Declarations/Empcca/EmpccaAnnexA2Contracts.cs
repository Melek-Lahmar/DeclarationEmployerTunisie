namespace DeclarationEmployer.Contracts.Declarations.Empcca;

public sealed class CreateEmpccaAnnexA2LineRequest
{
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public int AmountType { get; set; }
    public decimal GrossProfessionalAmount { get; set; }
    public decimal RealRegimeFeesAmount { get; set; }
    public decimal BoardAndSecuritiesAmount { get; set; }
    public decimal OccasionalWorkAmount { get; set; }
    public decimal RealEstateCapitalGainAmount { get; set; }
    public decimal HotelRentAmount { get; set; }
    public decimal ArtistRemunerationAmount { get; set; }
    public decimal PublicSectorVatWithheldAmount { get; set; }
    public decimal WithheldAmount { get; set; }
    public decimal NetPaidAmount { get; set; }
}

public sealed class EmpccaAnnexA2LineDto
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public EmpccaBeneficiaryInput Beneficiary { get; set; } = new();
    public CreateEmpccaAnnexA2LineRequest Details { get; set; } = new();
}

public sealed class EmpccaAnnexA2SummaryDto
{
    public int LineCount { get; set; }
    public decimal GrossProfessionalTotal { get; set; }
    public decimal WithheldTotal { get; set; }
    public decimal NetPaidTotal { get; set; }
}
