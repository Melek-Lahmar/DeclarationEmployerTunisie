using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Archive;

namespace DeclarationEmployer.Desktop.Services;

public sealed class ArchiveApiClient
{
    private readonly HttpClient _httpClient;

    public ArchiveApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ArchiveDeclarationResultDto> ArchiveAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/declarations/{declarationId}/archive", null, cancellationToken);
        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ArchiveDeclarationResultDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken) ?? throw new InvalidOperationException("Reponse API archivage vide.");
    }
}
