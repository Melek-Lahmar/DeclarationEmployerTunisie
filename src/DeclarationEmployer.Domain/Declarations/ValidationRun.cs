namespace DeclarationEmployer.Domain.Declarations;

public sealed class ValidationRun
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public ValidationRunStatus Status { get; set; } = ValidationRunStatus.Running;

    public int BlockingCount { get; set; }

    public int WarningCount { get; set; }

    public int InfoCount { get; set; }

    public int Score { get; set; }

    public string? CreatedBy { get; set; }

    public EmployerDeclaration? Declaration { get; set; }

    public ICollection<ValidationResult> Results { get; set; } = new List<ValidationResult>();
}
