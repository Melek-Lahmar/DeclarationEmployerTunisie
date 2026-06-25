namespace DeclarationEmployer.Domain.Declarations;

public sealed class DeclarationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? UserName { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public EmployerDeclaration? Declaration { get; set; }
}
