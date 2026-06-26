using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationExportApiClient
{
    private readonly HttpClient _httpClient;

    public DeclarationExportApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DeclarationExportPreviewDto> PreviewExportAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/declarations/{declarationId}/export/preview",
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationExportPreviewDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres previsualisation export.");
    }

    public async Task<DeclarationExportResultDto> GenerateExportAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/export/generate",
            new GenerateDeclarationExportRequest
            {
                Format = "CSV"
            },
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationExportResultDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres generation export.");
    }
}
