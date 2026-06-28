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
    [HttpPut("A3/lines/{lineId:guid}")]
    public async Task<IActionResult> UpdateA3(Guid declarationId, Guid lineId, CreateEmpccaAnnexA3LineRequest request, CancellationToken ct) => Ok(await _service.UpdateA3LineAsync(declarationId, lineId, request, ct));
    [HttpDelete("A3/lines/{lineId:guid}")]
    public async Task<IActionResult> DeleteA3(Guid declarationId, Guid lineId, CancellationToken ct) { await _service.DeleteA3LineAsync(declarationId, lineId, ct); return NoContent(); }
    [HttpGet("A3/summary")]
    public async Task<IActionResult> GetA3Summary(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA3SummaryAsync(declarationId, ct));
    [HttpPost("A3/validate")]
    public async Task<IActionResult> ValidateA3(Guid declarationId, CancellationToken ct) => Ok(await _service.ValidateA3Async(declarationId, ct));
    [HttpGet("A4/lines")]
    public async Task<IActionResult> GetA4(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA4LinesAsync(declarationId, ct));
    [HttpPost("A4/lines")]
    public async Task<IActionResult> CreateA4(Guid declarationId, CreateEmpccaAnnexA4LineRequest request, CancellationToken ct) => Ok(await _service.CreateA4LineAsync(declarationId, request, ct));
    [HttpPut("A4/lines/{lineId:guid}")]
    public async Task<IActionResult> UpdateA4(Guid declarationId, Guid lineId, CreateEmpccaAnnexA4LineRequest request, CancellationToken ct) => Ok(await _service.UpdateA4LineAsync(declarationId, lineId, request, ct));
    [HttpDelete("A4/lines/{lineId:guid}")]
    public async Task<IActionResult> DeleteA4(Guid declarationId, Guid lineId, CancellationToken ct) { await _service.DeleteA4LineAsync(declarationId, lineId, ct); return NoContent(); }
    [HttpGet("A4/summary")]
    public async Task<IActionResult> GetA4Summary(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA4SummaryAsync(declarationId, ct));
    [HttpPost("A4/validate")]
    public async Task<IActionResult> ValidateA4(Guid declarationId, CancellationToken ct) => Ok(await _service.ValidateA4Async(declarationId, ct));
    [HttpGet("A6/lines")]
    public async Task<IActionResult> GetA6(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA6LinesAsync(declarationId, ct));
    [HttpPost("A6/lines")]
    public async Task<IActionResult> CreateA6(Guid declarationId, CreateEmpccaAnnexA6LineRequest request, CancellationToken ct) => Ok(await _service.CreateA6LineAsync(declarationId, request, ct));
    [HttpPut("A6/lines/{lineId:guid}")]
    public async Task<IActionResult> UpdateA6(Guid declarationId, Guid lineId, CreateEmpccaAnnexA6LineRequest request, CancellationToken ct) => Ok(await _service.UpdateA6LineAsync(declarationId, lineId, request, ct));
    [HttpDelete("A6/lines/{lineId:guid}")]
    public async Task<IActionResult> DeleteA6(Guid declarationId, Guid lineId, CancellationToken ct) { await _service.DeleteA6LineAsync(declarationId, lineId, ct); return NoContent(); }
    [HttpGet("A6/summary")]
    public async Task<IActionResult> GetA6Summary(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA6SummaryAsync(declarationId, ct));
    [HttpPost("A6/validate")]
    public async Task<IActionResult> ValidateA6(Guid declarationId, CancellationToken ct) => Ok(await _service.ValidateA6Async(declarationId, ct));
    [HttpGet("A7/lines")]
    public async Task<IActionResult> GetA7(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA7LinesAsync(declarationId, ct));
    [HttpPost("A7/lines")]
    public async Task<IActionResult> CreateA7(Guid declarationId, CreateEmpccaAnnexA7LineRequest request, CancellationToken ct) => Ok(await _service.CreateA7LineAsync(declarationId, request, ct));
    [HttpPut("A7/lines/{lineId:guid}")]
    public async Task<IActionResult> UpdateA7(Guid declarationId, Guid lineId, CreateEmpccaAnnexA7LineRequest request, CancellationToken ct) => Ok(await _service.UpdateA7LineAsync(declarationId, lineId, request, ct));
    [HttpDelete("A7/lines/{lineId:guid}")]
    public async Task<IActionResult> DeleteA7(Guid declarationId, Guid lineId, CancellationToken ct) { await _service.DeleteA7LineAsync(declarationId, lineId, ct); return NoContent(); }
    [HttpGet("A7/summary")]
    public async Task<IActionResult> GetA7Summary(Guid declarationId, CancellationToken ct) => Ok(await _service.GetA7SummaryAsync(declarationId, ct));
    [HttpPost("A7/validate")]
    public async Task<IActionResult> ValidateA7(Guid declarationId, CancellationToken ct) => Ok(await _service.ValidateA7Async(declarationId, ct));
}
