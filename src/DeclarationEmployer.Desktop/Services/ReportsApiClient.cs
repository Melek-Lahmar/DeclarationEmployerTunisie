using System.Net.Http;

namespace DeclarationEmployer.Desktop.Services;

public sealed class ReportsApiClient
{
    private readonly HttpClient _httpClient;

    public ReportsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<byte[]> GetSummaryReportAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        return GetReportAsync($"api/declarations/{declarationId}/reports/summary", cancellationToken);
    }

    public Task<byte[]> GetGenerationReportAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        return GetReportAsync($"api/declarations/{declarationId}/reports/generation", cancellationToken);
    }

    private async Task<byte[]> GetReportAsync(string url, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
