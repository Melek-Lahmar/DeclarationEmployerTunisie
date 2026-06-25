using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/declarations")]
public sealed class DeclarationsController : ControllerBase
{
    private readonly IDeclarationsService _declarationsService;

    public DeclarationsController(IDeclarationsService declarationsService)
    {
        _declarationsService = declarationsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeclarationDto>>> GetAll(
        [FromQuery] Guid? clientId = null,
        [FromQuery] Guid? fiscalYearId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var declarations = await _declarationsService.GetAllAsync(
            clientId,
            fiscalYearId,
            status,
            cancellationToken);

        return Ok(declarations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeclarationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaration = await _declarationsService.GetByIdAsync(id, cancellationToken);

        if (declaration is null)
        {
            return NotFound(new { message = "Declaration introuvable." });
        }

        return Ok(declaration);
    }

    [HttpPost]
    public async Task<ActionResult<DeclarationDto>> Create(
        CreateDeclarationRequest request,
        CancellationToken cancellationToken = default)
    {
        var declaration = await _declarationsService.CreateAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = declaration.Id }, declaration);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeclarationDto>> Update(
        Guid id,
        UpdateDeclarationRequest request,
        CancellationToken cancellationToken = default)
    {
        var declaration = await _declarationsService.UpdateAsync(
            id,
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(declaration);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _declarationsService.DeleteAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<DeclarationSummaryDto>> GetSummary(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var summary = await _declarationsService.GetSummaryAsync(id, cancellationToken);

        if (summary is null)
        {
            return NotFound(new { message = "Declaration introuvable." });
        }

        return Ok(summary);
    }

    [HttpGet("{id:guid}/events")]
    public async Task<ActionResult<IReadOnlyList<DeclarationEventDto>>> GetEvents(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaration = await _declarationsService.GetByIdAsync(id, cancellationToken);

        if (declaration is null)
        {
            return NotFound(new { message = "Declaration introuvable." });
        }

        var events = await _declarationsService.GetEventsAsync(id, cancellationToken);
        return Ok(events);
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<ActionResult<DeclarationDto>> Lock(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaration = await _declarationsService.LockAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(declaration);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<DeclarationDto>> Close(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaration = await _declarationsService.CloseAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(declaration);
    }
}
