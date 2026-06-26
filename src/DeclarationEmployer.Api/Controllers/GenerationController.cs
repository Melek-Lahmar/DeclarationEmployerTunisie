using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Generation;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
public sealed class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;

    public GenerationController(IGenerationService generationService)
    {
        _generationService = generationService;
    }

    [HttpPost("api/declarations/{declarationId:guid}/generate")]
    public async Task<ActionResult<GenerationResultDto>> Generate(
        Guid declarationId,
        GenerateDeclarationFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _generationService.GenerateAsync(declarationId, request, cancellationToken);
        return Ok(result);
    }
}
