using DeclarationEmployer.Application.Fiscal;
using DeclarationEmployer.Contracts.Fiscal;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/fiscal")]
public sealed class FiscalReferenceController : ControllerBase
{
    private readonly IFiscalReferenceService _fiscalReferenceService;

    public FiscalReferenceController(IFiscalReferenceService fiscalReferenceService)
    {
        _fiscalReferenceService = fiscalReferenceService;
    }

    [HttpGet("rule-sets")]
    public async Task<ActionResult<IReadOnlyList<FiscalRuleSetDto>>> GetRuleSets(
        [FromQuery] int? year = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var ruleSets = await _fiscalReferenceService.GetRuleSetsAsync(year, activeOnly, cancellationToken);
        return Ok(ruleSets);
    }

    [HttpGet("rule-sets/{id:guid}")]
    public async Task<ActionResult<FiscalRuleSetDto>> GetRuleSet(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ruleSet = await _fiscalReferenceService.GetRuleSetAsync(id, cancellationToken);

        if (ruleSet is null)
        {
            return NotFound(new { message = "Referentiel fiscal introuvable." });
        }

        return Ok(ruleSet);
    }

    [HttpGet("annexes")]
    public async Task<ActionResult<IReadOnlyList<AnnexDefinitionDto>>> GetAnnexes(
        [FromQuery] int? year = null,
        [FromQuery] string? ruleSetCode = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var annexes = await _fiscalReferenceService.GetAnnexesAsync(
            year,
            ruleSetCode,
            activeOnly,
            cancellationToken);

        return Ok(annexes);
    }

    [HttpGet("annexes/{annexCode}")]
    public async Task<ActionResult<AnnexDefinitionDto>> GetAnnex(
        string annexCode,
        [FromQuery] int year = 2025,
        CancellationToken cancellationToken = default)
    {
        var annex = await _fiscalReferenceService.GetAnnexAsync(year, annexCode, cancellationToken);

        if (annex is null)
        {
            return NotFound(new { message = "Annexe fiscale introuvable." });
        }

        return Ok(annex);
    }

    [HttpGet("annexes/{annexCode}/fields")]
    public async Task<ActionResult<IReadOnlyList<FiscalFieldDefinitionDto>>> GetFields(
        string annexCode,
        [FromQuery] int year = 2025,
        CancellationToken cancellationToken = default)
    {
        var fields = await _fiscalReferenceService.GetFieldsAsync(year, annexCode, cancellationToken);
        return Ok(fields);
    }

    [HttpGet("rates")]
    public async Task<ActionResult<IReadOnlyList<FiscalRateDefinitionDto>>> GetRates(
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        var rates = await _fiscalReferenceService.GetRatesAsync(year, cancellationToken);
        return Ok(rates);
    }

    [HttpGet("readiness")]
    public async Task<ActionResult<FiscalReadinessDto>> GetReadiness(
        [FromQuery] int year = 2025,
        CancellationToken cancellationToken = default)
    {
        var readiness = await _fiscalReferenceService.GetReadinessAsync(year, cancellationToken);
        return Ok(readiness);
    }
}
