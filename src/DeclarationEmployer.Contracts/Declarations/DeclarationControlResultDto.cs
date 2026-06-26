namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationControlResultDto
{
    public Guid DeclarationId { get; set; }

    public int CheckedLinesCount { get; set; }

    public int BlockingAnomaliesCount { get; set; }

    public int WarningAnomaliesCount { get; set; }

    public int InfoAnomaliesCount { get; set; }

    public string DeclarationStatus { get; set; } = string.Empty;

    public IReadOnlyList<DeclarationAnomalyDto> Anomalies { get; set; } = [];
}
