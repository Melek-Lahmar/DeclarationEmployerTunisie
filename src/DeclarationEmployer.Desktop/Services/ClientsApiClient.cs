using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DeclarationEmployer.Contracts.Cabinet;

namespace DeclarationEmployer.Desktop.Services;

public sealed class ClientsApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ClientsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ClientCompanyDto>> GetClientsAsync(
        bool includeInactive = false,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (includeInactive)
        {
            query.Add("includeInactive=true");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status)}");
        }

        var url = query.Count == 0
            ? "api/clients"
            : $"api/clients?{string.Join("&", query)}";

        var result = await _httpClient.GetFromJsonAsync<List<ClientCompanyDto>>(
            url,
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<ClientCompanyDto> CreateAsync(
        CreateClientCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/clients",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ClientCompanyDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Réponse API vide après création.");
    }

    public async Task<ClientCompanyDto> UpdateAsync(
        Guid id,
        UpdateClientCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/clients/{id}",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ClientCompanyDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Réponse API vide après modification.");
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/clients/{id}",
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

