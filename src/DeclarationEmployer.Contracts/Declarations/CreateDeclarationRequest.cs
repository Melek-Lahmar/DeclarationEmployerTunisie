namespace DeclarationEmployer.Contracts.Declarations;

public sealed class CreateDeclarationRequest
{
    public Guid ClientCompanyId { get; set; }

    public Guid FiscalYearId { get; set; }

    public int ActCode { get; set; }

    public string? Title { get; set; }

    public string? Notes { get; set; }
}
