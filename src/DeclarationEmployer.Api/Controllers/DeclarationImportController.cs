using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Import;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/declarations/{declarationId:guid}/import/excel")]
public sealed class DeclarationImportController : ControllerBase
{
    private readonly IDeclarationImportService _declarationImportService;

    public DeclarationImportController(IDeclarationImportService declarationImportService)
    {
        _declarationImportService = declarationImportService;
    }

    [HttpPost("preview")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ExcelImportPreviewDto>> Preview(
        Guid declarationId,
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Le fichier Excel est obligatoire." });
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Seuls les fichiers .xlsx sont autorises." });
        }

        await using var stream = file.OpenReadStream();
        var preview = await _declarationImportService.PreviewAsync(
            declarationId,
            stream,
            file.FileName,
            cancellationToken);

        return Ok(preview);
    }

    [HttpPost("commit")]
    public async Task<ActionResult<ExcelImportCommitResultDto>> Commit(
        Guid declarationId,
        ExcelImportCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _declarationImportService.CommitAsync(
            declarationId,
            request,
            cancellationToken);

        return Ok(result);
    }
}
