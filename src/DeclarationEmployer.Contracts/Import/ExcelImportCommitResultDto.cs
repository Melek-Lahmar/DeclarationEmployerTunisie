namespace DeclarationEmployer.Contracts.Import;

public sealed class ExcelImportCommitResultDto
{
    public Guid DeclarationId { get; set; }

    public int ImportedRows { get; set; }

    public int SkippedRows { get; set; }

    public int CreatedBeneficiaries { get; set; }

    public int CreatedLines { get; set; }

    public int CreatedAnomalies { get; set; }
}
