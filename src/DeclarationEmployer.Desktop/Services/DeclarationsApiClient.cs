using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DeclarationsApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public DeclarationsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DeclarationDto>> GetDeclarationsAsync(
        Guid? clientId = null,
        Guid? fiscalYearId = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (clientId.HasValue)
        {
            query.Add($"clientId={clientId.Value}");
        }

        if (fiscalYearId.HasValue)
        {
            query.Add($"fiscalYearId={fiscalYearId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status)}");
        }

        var url = query.Count == 0
            ? "api/declarations"
            : $"api/declarations?{string.Join("&", query)}";

        var result = await _httpClient.GetFromJsonAsync<List<DeclarationDto>>(
            url,
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DeclarationDto> CreateAsync(
        CreateDeclarationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/declarations",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<DeclarationDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres creation de la declaration.");
    }

    public async Task<DeclarationSummaryDto?> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DeclarationSummaryDto>(
            $"api/declarations/{id}/summary",
            JsonOptions,
            cancellationToken);
    }

    public async Task<IReadOnlyList<DeclarationEventDto>> GetEventsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DeclarationEventDto>>(
            $"api/declarations/{id}/events",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DeclarationDto> UpdateAsync(
        Guid id,
        UpdateDeclarationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/declarations/{id}",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<DeclarationDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres modification de la declaration.");
    }

    public async Task<DeclarationDto> LockAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/declarations/{id}/lock",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<DeclarationDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres verrouillage.");
    }

    public async Task<DeclarationDto> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/declarations/{id}/close",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<DeclarationDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres cloture.");
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/declarations/{id}",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"Erreur API {(int)response.StatusCode} - {response.ReasonPhrase} : {content}");
    }
}
