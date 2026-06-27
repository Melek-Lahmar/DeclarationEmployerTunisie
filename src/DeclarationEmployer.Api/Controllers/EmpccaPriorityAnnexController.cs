using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.Empcca;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/declarations/{declarationId:guid}/empcca/annexes")]
public sealed class EmpccaPriorityAnnexController : ControllerBase
{
    private readonly IEmpccaPriorityAnnexService _service;

    public EmpccaPriorityAnnexController(IEmpccaPriorityAnnexService service) => _service = service;

    [HttpGet("A1/lines")]
    public async Task<ActionResult<IReadOnlyList<EmpccaAnnexA1LineDto>>> GetA1Lines(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetA1LinesAsync(declarationId, cancellationToken));

    [HttpPost("A1/lines")]
    public async Task<ActionResult<EmpccaAnnexA1LineDto>> CreateA1Line(
        Guid declarationId, CreateEmpccaAnnexA1LineRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateA1LineAsync(declarationId, request, cancellationToken));

    [HttpGet("A1/summary")]
    public async Task<ActionResult<EmpccaAnnexA1SummaryDto>> GetA1Summary(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetA1SummaryAsync(declarationId, cancellationToken));

    [HttpPost("A1/validate")]
    public async Task<ActionResult<EmpccaAnnexValidationDto>> ValidateA1(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.ValidateA1Async(declarationId, cancellationToken));

    [HttpGet("A2/lines")]
    public async Task<ActionResult<IReadOnlyList<EmpccaAnnexA2LineDto>>> GetA2Lines(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetA2LinesAsync(declarationId, cancellationToken));

    [HttpPost("A2/lines")]
    public async Task<ActionResult<EmpccaAnnexA2LineDto>> CreateA2Line(
        Guid declarationId, CreateEmpccaAnnexA2LineRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateA2LineAsync(declarationId, request, cancellationToken));

    [HttpGet("A2/summary")]
    public async Task<ActionResult<EmpccaAnnexA2SummaryDto>> GetA2Summary(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetA2SummaryAsync(declarationId, cancellationToken));

    [HttpPost("A2/validate")]
    public async Task<ActionResult<EmpccaAnnexValidationDto>> ValidateA2(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.ValidateA2Async(declarationId, cancellationToken));

    [HttpGet("A5/lines")]
    public async Task<ActionResult<IReadOnlyList<EmpccaAnnexA5LineDto>>> GetA5Lines(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetA5LinesAsync(declarationId, cancellationToken));

    [HttpPost("A5/lines")]
    public async Task<ActionResult<EmpccaAnnexA5LineDto>> CreateA5Line(
        Guid declarationId, CreateEmpccaAnnexA5LineRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateA5LineAsync(declarationId, request, cancellationToken));

    [HttpGet("A5/summary")]
    public async Task<ActionResult<EmpccaAnnexA5SummaryDto>> GetA5Summary(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetA5SummaryAsync(declarationId, cancellationToken));

    [HttpPost("A5/validate")]
    public async Task<ActionResult<EmpccaAnnexValidationDto>> ValidateA5(Guid declarationId, CancellationToken cancellationToken) =>
        Ok(await _service.ValidateA5Async(declarationId, cancellationToken));
}
