using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationBeneficiariesApiClient
{
    private readonly HttpClient _httpClient;

    public DeclarationBeneficiariesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DeclarationBeneficiaryDto>> GetByDeclarationAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DeclarationBeneficiaryDto>>(
            $"api/declarations/{declarationId}/beneficiaries",
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DeclarationBeneficiaryDto> CreateAsync(
        Guid declarationId,
        CreateDeclarationBeneficiaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/beneficiaries",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationBeneficiaryDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres creation du beneficiaire.");
    }

    public async Task<DeclarationBeneficiaryDto> UpdateAsync(
        Guid declarationId,
        Guid beneficiaryId,
        UpdateDeclarationBeneficiaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/declarations/{declarationId}/beneficiaries/{beneficiaryId}",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<DeclarationBeneficiaryDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres mise a jour du beneficiaire.");
    }

    public async Task DeleteAsync(
        Guid declarationId,
        Guid beneficiaryId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/declarations/{declarationId}/beneficiaries/{beneficiaryId}",
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
    }
}
