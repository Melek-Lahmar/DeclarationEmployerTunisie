using DeclarationEmployer.Application.Dashboard;
using DeclarationEmployer.Contracts.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        CancellationToken cancellationToken = default)
    {
        return Ok(await _dashboardService.GetSummaryAsync(cancellationToken));
    }

    [HttpGet("recent-actions")]
    public async Task<ActionResult<IReadOnlyList<RecentActionDto>>> GetRecentActions(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _dashboardService.GetRecentActionsAsync(count, cancellationToken));
    }

    [HttpGet("progress")]
    public async Task<ActionResult<DeclarationProgressDto>> GetProgress(
        [FromQuery] Guid? clientId = null,
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _dashboardService.GetProgressAsync(clientId, year, cancellationToken));
    }
}
