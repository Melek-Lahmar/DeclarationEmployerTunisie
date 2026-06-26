using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}")]
public sealed class DeclarationControlController : ControllerBase
{
    private readonly IDeclarationControlService _declarationControlService;

    public DeclarationControlController(IDeclarationControlService declarationControlService)
    {
        _declarationControlService = declarationControlService;
    }

    [HttpPost("control")]
    public async Task<ActionResult<DeclarationControlResultDto>> Control(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _declarationControlService.ControlAsync(
            declarationId,
            cancellationToken);

        return Ok(result);
    }
}
