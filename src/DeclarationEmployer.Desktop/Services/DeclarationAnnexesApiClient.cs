using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationAnnexesApiClient
{
    private readonly HttpClient _httpClient;

    public DeclarationAnnexesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DeclarationAnnexDto>> GetByDeclarationAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DeclarationAnnexDto>>(
            $"api/declarations/{declarationId}/annexes",
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DeclarationAnnexDto> CreateAsync(
        Guid declarationId,
        CreateDeclarationAnnexRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/annexes",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationAnnexDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres creation de l'annexe.");
    }

    public async Task<DeclarationAnnexDto> UpdateAsync(
        Guid declarationId,
        Guid annexId,
        UpdateDeclarationAnnexRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/declarations/{declarationId}/annexes/{annexId}",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationAnnexDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres mise a jour de l'annexe.");
    }

    public async Task DeleteAsync(
        Guid declarationId,
        Guid annexId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/declarations/{declarationId}/annexes/{annexId}",
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
    }
}
