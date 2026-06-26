namespace DeclarationEmployer.Domain.Declarations;

public sealed class DeclarationAnomaly
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public DeclarationAnomalySeverity Severity { get; set; } = DeclarationAnomalySeverity.Blocking;

    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public bool IsResolved { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ResolvedAt { get; set; }

    public string? ResolvedBy { get; set; }

    public EmployerDeclaration? Declaration { get; set; }
}
