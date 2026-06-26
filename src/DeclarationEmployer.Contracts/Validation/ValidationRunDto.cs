namespace DeclarationEmployer.Contracts.Validation;

public sealed class ValidationRunDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public int BlockingCount { get; set; }

    public int WarningCount { get; set; }

    public int InfoCount { get; set; }

    public int Score { get; set; }

    public string? CreatedBy { get; set; }
}
