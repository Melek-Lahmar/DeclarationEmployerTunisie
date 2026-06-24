using DeclarationEmployer.Application.Cabinet;
using DeclarationEmployer.Contracts.Cabinet;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class FiscalYearsController : ControllerBase
{
    private readonly IFiscalYearsService _fiscalYearsService;

    public FiscalYearsController(IFiscalYearsService fiscalYearsService)
    {
        _fiscalYearsService = fiscalYearsService;
    }

    [HttpGet("fiscal-years")]
    public async Task<ActionResult<IReadOnlyList<FiscalYearDto>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var fiscalYears = await _fiscalYearsService.GetAllAsync(cancellationToken);
        return Ok(fiscalYears);
    }

    [HttpGet("fiscal-years/{id:guid}")]
    public async Task<ActionResult<FiscalYearDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fiscalYear = await _fiscalYearsService.GetByIdAsync(id, cancellationToken);

        if (fiscalYear is null)
        {
            return NotFound(new { message = "Exercice fiscal introuvable." });
        }

        return Ok(fiscalYear);
    }

    [HttpGet("clients/{clientId:guid}/fiscal-years")]
    public async Task<ActionResult<IReadOnlyList<FiscalYearDto>>> GetByClient(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var fiscalYears = await _fiscalYearsService.GetByClientAsync(clientId, cancellationToken);
        return Ok(fiscalYears);
    }

    [HttpPost("clients/{clientId:guid}/fiscal-years")]
    public async Task<ActionResult<FiscalYearDto>> Create(
        Guid clientId,
        CreateFiscalYearRequest request,
        CancellationToken cancellationToken = default)
    {
        var fiscalYear = await _fiscalYearsService.CreateAsync(
            clientId,
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = fiscalYear.Id }, fiscalYear);
    }

    [HttpPut("fiscal-years/{id:guid}")]
    public async Task<ActionResult<FiscalYearDto>> Update(
        Guid id,
        UpdateFiscalYearRequest request,
        CancellationToken cancellationToken = default)
    {
        var fiscalYear = await _fiscalYearsService.UpdateAsync(
            id,
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(fiscalYear);
    }

    [HttpPost("fiscal-years/{id:guid}/close")]
    public async Task<ActionResult<FiscalYearDto>> Close(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fiscalYear = await _fiscalYearsService.CloseAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(fiscalYear);
    }

    [HttpPost("fiscal-years/{id:guid}/reopen")]
    public async Task<ActionResult<FiscalYearDto>> Reopen(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fiscalYear = await _fiscalYearsService.ReopenAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(fiscalYear);
    }

    [HttpGet("fiscal-years/{id:guid}/status")]
    public async Task<ActionResult<FiscalYearStatusDto>> GetStatus(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var status = await _fiscalYearsService.GetStatusAsync(id, cancellationToken);

        if (status is null)
        {
            return NotFound(new { message = "Exercice fiscal introuvable." });
        }

        return Ok(status);
    }

    [HttpGet("fiscal-years/{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<FiscalYearHistoryDto>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fiscalYear = await _fiscalYearsService.GetByIdAsync(id, cancellationToken);

        if (fiscalYear is null)
        {
            return NotFound(new { message = "Exercice fiscal introuvable." });
        }

        var history = await _fiscalYearsService.GetHistoryAsync(id, cancellationToken);
        return Ok(history);
    }
}
