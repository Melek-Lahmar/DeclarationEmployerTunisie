using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations.Empcca;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/declarations/{declarationId:guid}/empcca/annexes")]
public sealed class EmpccaRemainingAnnexController : ControllerBase
{
    private readonly IEmpccaRemainingAnnexService _service;
    public EmpccaRemainingAnnexController(IEmpccaRemainingAnnexService service) => _service = service;

    [HttpGet("A3/lines")]
    public async Task<IActionResult> GetA3(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA3LinesAsync(declarationId, ct));
    [HttpPost("A3/lines")]
    public async Task<IActionResult> CreateA3(Guid declarationId, CreateEmpccaAnnexA3LineRequest request, CancellationToken ct) => Ok(await _service.CreateA3LineAsync(declarationId, request, ct));
    [HttpGet("A4/lines")]
    public async Task<IActionResult> GetA4(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA4LinesAsync(declarationId, ct));
    [HttpPost("A4/lines")]
    public async Task<IActionResult> CreateA4(Guid declarationId, CreateEmpccaAnnexA4LineRequest request, CancellationToken ct) => Ok(await _service.CreateA4LineAsync(declarationId, request, ct));
    [HttpGet("A6/lines")]
    public async Task<IActionResult> GetA6(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA6LinesAsync(declarationId, ct));
    [HttpPost("A6/lines")]
    public async Task<IActionResult> CreateA6(Guid declarationId, CreateEmpccaAnnexA6LineRequest request, CancellationToken ct) => Ok(await _service.CreateA6LineAsync(declarationId, request, ct));
    [HttpGet("A7/lines")]
    public async Task<IActionResult> GetA7(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA7LinesAsync(declarationId, ct));
    [HttpPost("A7/lines")]
    public async Task<IActionResult> CreateA7(Guid declarationId, CreateEmpccaAnnexA7LineRequest request, CancellationToken ct) => Ok(await _service.CreateA7LineAsync(declarationId, request, ct));
}
