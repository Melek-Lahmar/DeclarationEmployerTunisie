namespace DeclarationEmployer.Contracts.Dashboard;

public sealed class RecentActionDto
{
    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? Details { get; set; }

    public string? UserName { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
