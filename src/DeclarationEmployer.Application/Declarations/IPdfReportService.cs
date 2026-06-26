namespace DeclarationEmployer.Application.Declarations;

public interface IPdfReportService
{
    Task<(byte[] Content, string FileName)> BuildDeclarationSummaryAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)> BuildAnnexA1Async(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)> BuildValidationErrorsAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)> BuildGenerationReportAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);
}
