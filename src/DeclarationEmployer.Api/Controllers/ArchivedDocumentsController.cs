using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
public sealed class ArchivedDocumentsController : ControllerBase
{
    private readonly IArchivedDocumentsService _service;

    public ArchivedDocumentsController(IArchivedDocumentsService service)
    {
        _service = service;
    }

    [HttpGet("api/declarations/{declarationId:guid}/archive")]
    public async Task<ActionResult<IReadOnlyList<ArchivedDocumentDto>>> GetByDeclaration(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _service.GetByDeclarationAsync(declarationId, cancellationToken);
        return Ok(documents);
    }

    [HttpGet("api/archives")]
    public async Task<ActionResult<IReadOnlyList<ArchivedDocumentDto>>> GetByClientAndYear(
        [FromQuery] Guid? clientId = null,
        [FromQuery] int? year = null,
        CancellationToken cancellationToken = default)
    {
        var documents = await _service.GetByClientAndYearAsync(clientId, year, cancellationToken);
        return Ok(documents);
    }
}
