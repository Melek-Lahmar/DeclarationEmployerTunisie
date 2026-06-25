namespace DeclarationEmployer.Contracts.Dashboard;

public sealed class DashboardSummaryDto
{
    public int ClientsCount { get; set; }

    public int ActiveClientsCount { get; set; }

    public int InactiveClientsCount { get; set; }

    public int FiscalYearsCount { get; set; }

    public int OpenFiscalYearsCount { get; set; }

    public int ClosedFiscalYearsCount { get; set; }

    public int BlockingAnomaliesCount { get; set; }

    public int GeneratedFilesCount { get; set; }

    public int ArchivedDeclarationsCount { get; set; }
}
