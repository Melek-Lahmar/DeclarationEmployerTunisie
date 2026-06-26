using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Generation;

namespace DeclarationEmployer.Desktop.Services;

public sealed class GenerationApiClient
{
    private readonly HttpClient _httpClient;

    public GenerationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GenerationResultDto> GenerateFoundationAsync(Guid declarationId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/generate",
            new GenerateDeclarationFilesRequest { OfficialModeRequested = false },
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<GenerationResultDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken) ?? throw new InvalidOperationException("Reponse API generation vide.");
    }
}
