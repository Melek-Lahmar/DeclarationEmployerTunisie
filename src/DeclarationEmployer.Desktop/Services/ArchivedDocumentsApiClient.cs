using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class ArchivedDocumentsApiClient
{
    private readonly HttpClient _httpClient;

    public ArchivedDocumentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ArchivedDocumentDto>> GetByDeclarationAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ArchivedDocumentDto>>(
            $"api/declarations/{declarationId}/archive",
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<IReadOnlyList<ArchivedDocumentDto>> GetByClientAndYearAsync(
        Guid? clientId,
        int? year,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (clientId.HasValue)
        {
            query.Add($"clientId={clientId.Value}");
        }

        if (year.HasValue)
        {
            query.Add($"year={year.Value}");
        }

        var url = query.Count == 0
            ? "api/archives"
            : $"api/archives?{string.Join("&", query)}";

        var result = await _httpClient.GetFromJsonAsync<List<ArchivedDocumentDto>>(
            url,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }
}
