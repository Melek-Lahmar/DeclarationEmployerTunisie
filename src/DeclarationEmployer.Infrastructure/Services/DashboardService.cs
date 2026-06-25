using DeclarationEmployer.Application.Dashboard;
using DeclarationEmployer.Contracts.Dashboard;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var clientsCount = await _db.Clients.CountAsync(cancellationToken);
        var activeClientsCount = await _db.Clients.CountAsync(x => x.IsActive, cancellationToken);
        var fiscalYearsCount = await _db.FiscalYears.CountAsync(cancellationToken);
        var closedFiscalYearsCount = await _db.FiscalYears.CountAsync(x => x.IsClosed, cancellationToken);

        return new DashboardSummaryDto
        {
            ClientsCount = clientsCount,
            ActiveClientsCount = activeClientsCount,
            InactiveClientsCount = clientsCount - activeClientsCount,
            FiscalYearsCount = fiscalYearsCount,
            OpenFiscalYearsCount = fiscalYearsCount - closedFiscalYearsCount,
            ClosedFiscalYearsCount = closedFiscalYearsCount,
            BlockingAnomaliesCount = 0,
            GeneratedFilesCount = 0,
            ArchivedDeclarationsCount = 0
        };
    }

    public async Task<IReadOnlyList<RecentActionDto>> GetRecentActionsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 50);

        return await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .Take(count)
            .Select(x => new RecentActionDto
            {
                Action = x.Action,
                EntityName = x.EntityName,
                Details = x.Details,
                UserName = x.UserName,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);
    }

    public Task<DeclarationProgressDto> GetProgressAsync(
        Guid? clientId,
        int? year,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DeclarationProgressDto
        {
            DeclarationsCount = 0,
            DraftCount = 0,
            ValidatedCount = 0,
            GeneratedCount = 0,
            ArchivedCount = 0
        });
    }
}
