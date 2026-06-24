namespace DeclarationEmployer.Contracts.Cabinet;

public sealed class FiscalYearStatusDto
{
    public Guid Id { get; set; }

    public int Year { get; set; }

    public bool IsClosed { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public string Status => IsClosed ? "Closed" : "Open";
}
