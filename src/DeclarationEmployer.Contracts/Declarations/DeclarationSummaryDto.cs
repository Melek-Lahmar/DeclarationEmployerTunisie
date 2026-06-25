namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationSummaryDto
{
    public DeclarationDto Declaration { get; set; } = new();

    public int AnnexesCount { get; set; }

    public int BlockingAnomaliesCount { get; set; }

    public int GeneratedFilesCount { get; set; }

    public string? LastEventAction { get; set; }

    public DateTimeOffset? LastEventAt { get; set; }
}
