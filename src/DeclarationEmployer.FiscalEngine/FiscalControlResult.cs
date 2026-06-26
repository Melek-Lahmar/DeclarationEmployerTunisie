namespace DeclarationEmployer.FiscalEngine;

public sealed class FiscalControlResult
{
    public Guid DeclarationId { get; set; }

    public int CheckedLinesCount { get; set; }

    public int BlockingIssuesCount { get; set; }

    public int WarningIssuesCount { get; set; }

    public int InfoIssuesCount { get; set; }

    public IReadOnlyList<FiscalControlIssue> Issues { get; set; } = [];
}
