using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Validation;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
public sealed class ValidationController : ControllerBase
{
    private readonly IValidationService _validationService;

    public ValidationController(IValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpPost("api/declarations/{declarationId:guid}/validate")]
    public async Task<ActionResult<ValidationRunSummaryDto>> ValidateDeclaration(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _validationService.RunAsync(declarationId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/declarations/{declarationId:guid}/validation-results")]
    public async Task<ActionResult<IReadOnlyList<ValidationResultDto>>> GetResults(
        Guid declarationId,
        [FromQuery] bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var results = await _validationService.GetResultsAsync(declarationId, includeResolved, cancellationToken);
        return Ok(results);
    }

    [HttpGet("api/declarations/{declarationId:guid}/errors")]
    public async Task<ActionResult<IReadOnlyList<ValidationResultDto>>> GetErrors(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var results = await _validationService.GetResultsAsync(declarationId, includeResolved: false, cancellationToken);
        return Ok(results.Where(x => x.Severity == "Blocking").ToList());
    }

    [HttpPost("api/validation-results/{id:guid}/mark-corrected")]
    public async Task<ActionResult<ValidationResultDto>> MarkCorrected(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _validationService.MarkCorrectedAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/validation-results/{id:guid}/ignore")]
    public async Task<ActionResult<ValidationResultDto>> Ignore(
        Guid id,
        IgnoreValidationResultRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _validationService.IgnoreAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
