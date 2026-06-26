using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/export")]
public sealed class DeclarationExportController : ControllerBase
{
    private readonly IDeclarationExportService _declarationExportService;

    public DeclarationExportController(IDeclarationExportService declarationExportService)
    {
        _declarationExportService = declarationExportService;
    }

    [HttpGet("preview")]
    public async Task<ActionResult<DeclarationExportPreviewDto>> Preview(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _declarationExportService.PreviewAsync(declarationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<DeclarationExportResultDto>> Generate(
        Guid declarationId,
        GenerateDeclarationExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _declarationExportService.GenerateAsync(declarationId, request, cancellationToken);
        return Ok(result);
    }
}
