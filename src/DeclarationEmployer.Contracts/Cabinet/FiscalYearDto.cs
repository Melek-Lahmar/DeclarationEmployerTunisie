namespace DeclarationEmployer.Contracts.Cabinet;

public sealed class FiscalYearDto
{
    public Guid Id { get; set; }

    public Guid ClientCompanyId { get; set; }

    public string? ClientCode { get; set; }

    public string? ClientRaisonSociale { get; set; }

    public int Year { get; set; }

    public bool IsClosed { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string Status => IsClosed ? "Closed" : "Open";
}
