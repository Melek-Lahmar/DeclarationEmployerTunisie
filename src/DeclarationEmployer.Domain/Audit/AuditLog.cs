namespace DeclarationEmployer.Domain.Audit;

public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? UserName { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
