using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Contracts.Common;

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

    public async Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/clients/{id}/deactivate",
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
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
        var message = TryBuildFriendlyMessage(content);

        throw new InvalidOperationException(message);
    }

    private static string TryBuildFriendlyMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "L'API a retourné une erreur sans détail.";
        }

        try
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<JsonElement>>(content, JsonOptions);
            var error = apiResponse?.Error;

            if (error is null)
            {
                return content;
            }

            var lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(error.Message))
            {
                lines.Add(error.Message);
            }

            var details = error.ValidationErrors ?? error.Details;
            if (details is not null)
            {
                foreach (var entry in details.OrderBy(x => x.Key))
                {
                    foreach (var detail in entry.Value)
                    {
                        if (!string.IsNullOrWhiteSpace(detail))
                        {
                            lines.Add($"- {detail}");
                        }
                    }
                }
            }

            if (lines.Count == 0)
            {
                return content;
            }

            return string.Join(Environment.NewLine, lines);
        }
        catch (JsonException)
        {
            return content;
        }
    }
}
