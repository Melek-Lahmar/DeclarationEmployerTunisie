using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationLinesApiClient
{
    private readonly HttpClient _httpClient;

    public DeclarationLinesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DeclarationLineDto>> GetByDeclarationAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DeclarationLineDto>>(
            $"api/declarations/{declarationId}/lines",
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DeclarationLineDto> CreateAsync(
        Guid declarationId,
        CreateDeclarationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/lines",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationLineDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres creation de la ligne.");
    }

    public async Task<DeclarationLineDto> UpdateAsync(
        Guid declarationId,
        Guid lineId,
        UpdateDeclarationLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/declarations/{declarationId}/lines/{lineId}",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationLineDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres mise a jour de la ligne.");
    }

    public async Task DeleteAsync(
        Guid declarationId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/declarations/{declarationId}/lines/{lineId}",
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
    }
}
