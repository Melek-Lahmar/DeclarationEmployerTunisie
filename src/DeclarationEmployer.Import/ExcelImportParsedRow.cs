namespace DeclarationEmployer.Import;

public sealed class ExcelImportParsedRow
{
    public int RowNumber { get; set; }

    public bool IsValid { get; set; }

    public string? BeneficiaryIdentifierType { get; set; }

    public string? BeneficiaryIdentifier { get; set; }

    public string? BeneficiaryName { get; set; }

    public string? OperationType { get; set; }

    public string? FiscalCategory { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? TaxableAmount { get; set; }

    public decimal? Rate { get; set; }

    public decimal? WithheldAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? DocumentReference { get; set; }

    public string? Notes { get; set; }

    public string? Address { get; set; }

    public string? Country { get; set; }

    public bool? IsResident { get; set; }
}
