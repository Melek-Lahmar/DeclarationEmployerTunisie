using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/anomalies")]
public sealed class DeclarationAnomaliesController : ControllerBase
{
    private readonly IDeclarationAnomaliesService _service;

    public DeclarationAnomaliesController(IDeclarationAnomaliesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeclarationAnomalyDto>>> GetByDeclaration(
        Guid declarationId,
        [FromQuery] string? severity = null,
        [FromQuery] bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var anomalies = await _service.GetByDeclarationAsync(
            declarationId,
            severity,
            includeResolved,
            cancellationToken);

        return Ok(anomalies);
    }

    [HttpPost("{anomalyId:guid}/resolve")]
    public async Task<ActionResult<DeclarationAnomalyDto>> Resolve(
        Guid declarationId,
        Guid anomalyId,
        ResolveDeclarationAnomalyRequest request,
        CancellationToken cancellationToken = default)
    {
        var anomaly = await _service.ResolveAsync(declarationId, anomalyId, request, cancellationToken);
        return Ok(anomaly);
    }
}
