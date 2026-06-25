namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationEventDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? UserName { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
