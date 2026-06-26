using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/annexes")]
public sealed class DeclarationAnnexesController : ControllerBase
{
    private readonly IDeclarationAnnexesService _service;

    public DeclarationAnnexesController(IDeclarationAnnexesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeclarationAnnexDto>>> GetByDeclaration(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var annexes = await _service.GetByDeclarationAsync(declarationId, cancellationToken);
        return Ok(annexes);
    }

    [HttpPost]
    public async Task<ActionResult<DeclarationAnnexDto>> Create(
        Guid declarationId,
        CreateDeclarationAnnexRequest request,
        CancellationToken cancellationToken = default)
    {
        var annex = await _service.CreateAsync(declarationId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByDeclaration), new { declarationId }, annex);
    }

    [HttpPut("{annexId:guid}")]
    public async Task<ActionResult<DeclarationAnnexDto>> Update(
        Guid declarationId,
        Guid annexId,
        UpdateDeclarationAnnexRequest request,
        CancellationToken cancellationToken = default)
    {
        var annex = await _service.UpdateAsync(declarationId, annexId, request, cancellationToken);
        return Ok(annex);
    }

    [HttpDelete("{annexId:guid}")]
    public async Task<IActionResult> Delete(
        Guid declarationId,
        Guid annexId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(declarationId, annexId, cancellationToken);
        return NoContent();
    }
}
