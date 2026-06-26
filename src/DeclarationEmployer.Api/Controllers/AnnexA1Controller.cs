using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.AnnexA1;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/declarations/{declarationId:guid}/annexes/A1")]
public sealed class AnnexA1Controller : ControllerBase
{
    private readonly IAnnexA1Service _annexA1Service;

    public AnnexA1Controller(IAnnexA1Service annexA1Service)
    {
        _annexA1Service = annexA1Service;
    }

    [HttpGet("lines")]
    public async Task<ActionResult<IReadOnlyList<AnnexA1LineDto>>> GetLines(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var lines = await _annexA1Service.GetLinesAsync(declarationId, cancellationToken);
        return Ok(lines);
    }

    [HttpPost("lines")]
    public async Task<ActionResult<AnnexA1LineDto>> CreateLine(
        Guid declarationId,
        CreateAnnexA1LineRequest request,
        CancellationToken cancellationToken = default)
    {
        var line = await _annexA1Service.CreateLineAsync(declarationId, request, cancellationToken);
        return CreatedAtAction(nameof(GetLines), new { declarationId }, line);
    }

    [HttpDelete("lines/{lineId:guid}")]
    public async Task<IActionResult> DeleteLine(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        await _annexA1Service.DeleteLineAsync(declarationId, lineId, cancellationToken);
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AnnexA1SummaryDto>> GetSummary(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _annexA1Service.GetSummaryAsync(declarationId, cancellationToken);
        return Ok(summary);
    }

    [HttpPost("validate-line/{lineId:guid}")]
    public async Task<ActionResult<AnnexA1LineValidationDto>> ValidateLine(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var result = await _annexA1Service.ValidateLineAsync(declarationId, lineId, cancellationToken);
        return Ok(result);
    }
}
