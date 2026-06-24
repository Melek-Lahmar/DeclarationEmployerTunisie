namespace DeclarationEmployer.Contracts.Cabinet;

public sealed class ClientSummaryDto
{
    public ClientCompanyDto Client { get; set; } = new();

    public int FiscalYearsCount { get; set; }

    public int? LastFiscalYear { get; set; }

    public int DeclarationsCount { get; set; }

    public string? LastAuditAction { get; set; }

    public DateTimeOffset? LastAuditAt { get; set; }
}
