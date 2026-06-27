namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationDto
{
    public Guid Id { get; set; }

    public Guid ClientCompanyId { get; set; }

    public string? ClientCode { get; set; }

    public string? ClientRaisonSociale { get; set; }

    public Guid FiscalYearId { get; set; }

    public int Year { get; set; }

    public int ActCode { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool IsLocked { get; set; }

    public DateTimeOffset? LockedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
