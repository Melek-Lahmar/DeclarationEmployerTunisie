namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationAnomalyDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public string Severity { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public bool IsResolved { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public string? ResolvedBy { get; set; }
}
