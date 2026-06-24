using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DeclarationEmployer.Contracts.Cabinet;

namespace DeclarationEmployer.Desktop.Services;

public sealed class FiscalYearsApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public FiscalYearsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<FiscalYearDto>> GetFiscalYearsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<FiscalYearDto>>(
            "api/fiscal-years",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<IReadOnlyList<FiscalYearDto>> GetFiscalYearsByClientAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<FiscalYearDto>>(
            $"api/clients/{clientId}/fiscal-years",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<FiscalYearDto> CreateAsync(
        Guid clientId,
        CreateFiscalYearRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/clients/{clientId}/fiscal-years",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<FiscalYearDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres creation de l'exercice.");
    }

    public async Task<FiscalYearDto> CloseAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/fiscal-years/{fiscalYearId}/close",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<FiscalYearDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres cloture de l'exercice.");
    }

    public async Task<FiscalYearDto> ReopenAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/fiscal-years/{fiscalYearId}/reopen",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<FiscalYearDto>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres reouverture de l'exercice.");
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
