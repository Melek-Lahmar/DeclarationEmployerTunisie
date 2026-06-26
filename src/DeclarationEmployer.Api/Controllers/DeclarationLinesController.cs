using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/lines")]
public sealed class DeclarationLinesController : ControllerBase
{
    private readonly IDeclarationLinesService _service;

    public DeclarationLinesController(IDeclarationLinesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeclarationLineDto>>> GetByDeclaration(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var lines = await _service.GetByDeclarationAsync(declarationId, cancellationToken);
        return Ok(lines);
    }

    [HttpPost]
    public async Task<ActionResult<DeclarationLineDto>> Create(
        Guid declarationId,
        CreateDeclarationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var line = await _service.CreateAsync(declarationId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByDeclaration), new { declarationId }, line);
    }

    [HttpPut("{lineId:guid}")]
    public async Task<ActionResult<DeclarationLineDto>> Update(
        Guid declarationId,
        Guid lineId,
        UpdateDeclarationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var line = await _service.UpdateAsync(declarationId, lineId, request, cancellationToken);
        return Ok(line);
    }

    [HttpDelete("{lineId:guid}")]
    public async Task<IActionResult> Delete(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(declarationId, lineId, cancellationToken);
        return NoContent();
    }
}
