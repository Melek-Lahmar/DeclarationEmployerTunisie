using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationControlApiClient
{
    private readonly HttpClient _httpClient;

    public DeclarationControlApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DeclarationControlResultDto> ControlAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/declarations/{declarationId}/control",
            content: null,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationControlResultDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres controle.");
    }
}
