using DeclarationEmployer.Domain.Cabinet;

namespace DeclarationEmployer.Domain.Declarations;

public sealed class EmployerDeclaration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientCompanyId { get; set; }

    public Guid FiscalYearId { get; set; }

    public int Year { get; set; }

    public DeclarationStatus Status { get; set; } = DeclarationStatus.Draft;

    public string Title { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool IsLocked { get; set; }

    public DateTimeOffset? LockedAt { get; set; }

    public string? LockedBy { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ClientCompany? ClientCompany { get; set; }

    public FiscalYear? FiscalYear { get; set; }
}
