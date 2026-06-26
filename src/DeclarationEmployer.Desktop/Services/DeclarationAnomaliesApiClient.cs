using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationAnomaliesApiClient
{
    private readonly HttpClient _httpClient;

    public DeclarationAnomaliesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DeclarationAnomalyDto>> GetByDeclarationAsync(
        Guid declarationId,
        string? severity = null,
        bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string> { $"includeResolved={includeResolved.ToString().ToLowerInvariant()}" };

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query.Add($"severity={Uri.EscapeDataString(severity)}");
        }

        var result = await _httpClient.GetFromJsonAsync<List<DeclarationAnomalyDto>>(
            $"api/declarations/{declarationId}/anomalies?{string.Join("&", query)}",
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DeclarationAnomalyDto> ResolveAsync(
        Guid declarationId,
        Guid anomalyId,
        ResolveDeclarationAnomalyRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/anomalies/{anomalyId}/resolve",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationAnomalyDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres resolution de l'anomalie.");
    }
}
