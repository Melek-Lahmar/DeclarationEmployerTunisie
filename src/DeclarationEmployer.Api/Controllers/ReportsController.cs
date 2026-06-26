using DeclarationEmployer.Application.Declarations;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
public sealed class ReportsController : ControllerBase
{
    private readonly IPdfReportService _pdfReportService;

    public ReportsController(IPdfReportService pdfReportService)
    {
        _pdfReportService = pdfReportService;
    }

    [HttpGet("api/declarations/{id:guid}/reports/summary")]
    public async Task<IActionResult> Summary(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _pdfReportService.BuildDeclarationSummaryAsync(id, cancellationToken);
        return File(report.Content, "application/pdf", report.FileName);
    }

    [HttpGet("api/declarations/{id:guid}/reports/annex-a1")]
    public async Task<IActionResult> AnnexA1(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _pdfReportService.BuildAnnexA1Async(id, cancellationToken);
        return File(report.Content, "application/pdf", report.FileName);
    }

    [HttpGet("api/declarations/{id:guid}/reports/errors")]
    public async Task<IActionResult> Errors(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _pdfReportService.BuildValidationErrorsAsync(id, cancellationToken);
        return File(report.Content, "application/pdf", report.FileName);
    }

    [HttpGet("api/declarations/{id:guid}/reports/generation")]
    public async Task<IActionResult> Generation(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _pdfReportService.BuildGenerationReportAsync(id, cancellationToken);
        return File(report.Content, "application/pdf", report.FileName);
    }
}
