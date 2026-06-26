using DeclarationEmployer.Application.Backup;
using DeclarationEmployer.Contracts.Backup;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/backups")]
public sealed class BackupsController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupsController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<BackupRecordDto>> Create(
        CreateBackupRequest request,
        CancellationToken cancellationToken = default)
    {
        var backup = await _backupService.CreateAsync(request, cancellationToken);
        return Ok(backup);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BackupRecordDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var backups = await _backupService.GetAllAsync(cancellationToken);
        return Ok(backups);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BackupRecordDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var backup = await _backupService.GetByIdAsync(id, cancellationToken);
        return backup is null ? NotFound(new { message = "Sauvegarde introuvable." }) : Ok(backup);
    }

    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult<BackupRecordDto>> Verify(Guid id, CancellationToken cancellationToken = default)
    {
        var backup = await _backupService.VerifyAsync(id, cancellationToken);
        return Ok(backup);
    }
}
