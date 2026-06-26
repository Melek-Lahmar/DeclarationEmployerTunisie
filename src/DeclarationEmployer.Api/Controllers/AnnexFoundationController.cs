using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.AnnexFoundation;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/declarations/{declarationId:guid}/annexes/{annexCode}")]
public sealed class AnnexFoundationController : ControllerBase
{
    private readonly IAnnexFoundationService _annexFoundationService;

    public AnnexFoundationController(IAnnexFoundationService annexFoundationService)
    {
        _annexFoundationService = annexFoundationService;
    }

    [HttpGet("lines")]
    public async Task<ActionResult<IReadOnlyList<AnnexFoundationLineDto>>> GetLines(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken = default)
    {
        var lines = await _annexFoundationService.GetLinesAsync(declarationId, annexCode, cancellationToken);
        return Ok(lines);
    }

    [HttpPost("lines")]
    public async Task<ActionResult<AnnexFoundationLineDto>> CreateLine(
        Guid declarationId,
        string annexCode,
        CreateAnnexFoundationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var line = await _annexFoundationService.CreateLineAsync(declarationId, annexCode, request, cancellationToken);
        return CreatedAtAction(nameof(GetLines), new { declarationId, annexCode }, line);
    }

    [HttpDelete("lines/{lineId:guid}")]
    public async Task<IActionResult> DeleteLine(
        Guid declarationId,
        string annexCode,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        await _annexFoundationService.DeleteLineAsync(declarationId, annexCode, lineId, cancellationToken);
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AnnexFoundationSummaryDto>> GetSummary(
        Guid declarationId,
        string annexCode,
        CancellationToken cancellationToken = default)
    {
        var summary = await _annexFoundationService.GetSummaryAsync(declarationId, annexCode, cancellationToken);
        return Ok(summary);
    }
}
