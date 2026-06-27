using DeclarationEmployer.Application.Cabinet;
using DeclarationEmployer.Contracts.Cabinet;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        if (page.HasValue || pageSize.HasValue)
        {
            var paged = await _clientsService.GetPagedAsync(
                includeInactive,
                search,
                status,
                page ?? 1,
                pageSize ?? 20,
                cancellationToken);

            return Ok(paged);
        }

        var clients = await _clientsService.GetAllAsync(
            includeInactive,
            search,
            status,
            cancellationToken);

        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientCompanyDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientsService.GetByIdAsync(id, cancellationToken);

        if (client is null)
        {
            return NotFound(new { message = "Société cliente introuvable." });
        }

        return Ok(client);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<ClientSummaryDto>> GetSummary(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var summary = await _clientsService.GetSummaryAsync(id, cancellationToken);

        if (summary is null)
        {
            return NotFound(new { message = "Societe cliente introuvable." });
        }

        return Ok(summary);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<ClientHistoryDto>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientsService.GetByIdAsync(id, cancellationToken);

        if (client is null)
        {
            return NotFound(new { message = "Societe cliente introuvable." });
        }

        var history = await _clientsService.GetHistoryAsync(id, cancellationToken);
        return Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<ClientCompanyDto>> Create(
        CreateClientCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientsService.CreateAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientCompanyDto>> Update(
        Guid id,
        UpdateClientCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var client = await _clientsService.UpdateAsync(
            id,
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return Ok(client);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _clientsService.DeactivateAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _clientsService.DeleteAsync(
            id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return NoContent();
    }
}
