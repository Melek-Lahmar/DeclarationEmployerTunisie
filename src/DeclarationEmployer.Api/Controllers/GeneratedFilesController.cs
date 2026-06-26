using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/generated-files")]
public sealed class GeneratedFilesController : ControllerBase
{
    private readonly IGeneratedFilesService _service;

    public GeneratedFilesController(IGeneratedFilesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GeneratedFileDto>>> GetByDeclaration(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var files = await _service.GetByDeclarationAsync(declarationId, cancellationToken);
        return Ok(files);
    }
}
