using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/beneficiaries")]
public sealed class DeclarationBeneficiariesController : ControllerBase
{
    private readonly IDeclarationBeneficiariesService _service;

    public DeclarationBeneficiariesController(IDeclarationBeneficiariesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeclarationBeneficiaryDto>>> GetByDeclaration(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var beneficiaries = await _service.GetByDeclarationAsync(declarationId, cancellationToken);
        return Ok(beneficiaries);
    }

    [HttpPost]
    public async Task<ActionResult<DeclarationBeneficiaryDto>> Create(
        Guid declarationId,
        CreateDeclarationBeneficiaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var beneficiary = await _service.CreateAsync(declarationId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByDeclaration), new { declarationId }, beneficiary);
    }

    [HttpPut("{beneficiaryId:guid}")]
    public async Task<ActionResult<DeclarationBeneficiaryDto>> Update(
        Guid declarationId,
        Guid beneficiaryId,
        UpdateDeclarationBeneficiaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var beneficiary = await _service.UpdateAsync(declarationId, beneficiaryId, request, cancellationToken);
        return Ok(beneficiary);
    }

    [HttpDelete("{beneficiaryId:guid}")]
    public async Task<IActionResult> Delete(
        Guid declarationId,
        Guid beneficiaryId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(declarationId, beneficiaryId, cancellationToken);
        return NoContent();
    }
}
