namespace DeclarationEmployer.Domain.Declarations.Empcca;

public sealed class AnnexA2Detail
{
    public Guid LineId { get; set; }
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
    public DeclarationLine? Line { get; set; }
}
