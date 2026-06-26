using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Archive;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
public sealed class ArchiveController : ControllerBase
{
    private readonly IArchiveService _archiveService;

    public ArchiveController(IArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    [HttpPost("api/declarations/{id:guid}/archive")]
    public async Task<ActionResult<ArchiveDeclarationResultDto>> Archive(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _archiveService.ArchiveAsync(id, cancellationToken);
        return Ok(result);
    }
}
