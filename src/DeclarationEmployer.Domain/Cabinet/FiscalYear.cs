namespace DeclarationEmployer.Domain.Cabinet;

public sealed class FiscalYear
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientCompanyId { get; set; }

    public int Year { get; set; }

    public bool IsClosed { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ClientCompany? ClientCompany { get; set; }
}
