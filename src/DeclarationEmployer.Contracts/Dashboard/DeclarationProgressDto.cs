namespace DeclarationEmployer.Contracts.Dashboard;

public sealed class DeclarationProgressDto
{
    public int DeclarationsCount { get; set; }

    public int DraftCount { get; set; }

    public int ValidatedCount { get; set; }

    public int GeneratedCount { get; set; }

    public int ArchivedCount { get; set; }
}
