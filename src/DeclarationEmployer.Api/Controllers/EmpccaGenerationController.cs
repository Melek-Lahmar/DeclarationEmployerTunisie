using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Generation;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/declarations/{declarationId:guid}/empcca")]
public sealed class EmpccaGenerationController : ControllerBase
{
    private readonly IEmpccaGenerationPreviewService _service;

    public EmpccaGenerationController(IEmpccaGenerationPreviewService service) => _service = service;

    [HttpGet("generation-preview")]
    public async Task<ActionResult<EmpccaGenerationPreviewDto>> Preview(
        Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.PreviewAsync(declarationId, cancellationToken));

    [HttpPost("validate-official")]
    public async Task<ActionResult<EmpccaGenerationPreviewDto>> ValidateOfficial(
        Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.PreviewAsync(declarationId, cancellationToken));
}
