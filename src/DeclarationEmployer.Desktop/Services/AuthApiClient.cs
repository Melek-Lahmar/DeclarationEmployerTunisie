using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DeclarationEmployer.Contracts.Auth;
using DeclarationEmployer.Contracts.Users;

namespace DeclarationEmployer.Desktop.Services;

public sealed class AuthApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres login.");
    }

    public async Task<UserDto> MeAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<UserDto>(
            "api/auth/me",
            JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide pour l'utilisateur courant.");
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
