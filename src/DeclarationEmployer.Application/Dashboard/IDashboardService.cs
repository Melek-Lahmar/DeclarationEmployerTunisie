using DeclarationEmployer.Contracts.Dashboard;

namespace DeclarationEmployer.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentActionDto>> GetRecentActionsAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<DeclarationProgressDto> GetProgressAsync(
        Guid? clientId,
        int? year,
        CancellationToken cancellationToken = default);
}
