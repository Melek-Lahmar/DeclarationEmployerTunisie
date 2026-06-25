namespace DeclarationEmployer.Contracts.Cabinet;

public sealed class ClientHistoryDto
{
    public string Action { get; set; } = string.Empty;

    public string? Details { get; set; }

    public string? UserName { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
