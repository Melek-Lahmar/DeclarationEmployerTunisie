using System.Net.Http;
using System.Text.Json;

namespace DeclarationEmployer.Desktop.Services;

internal static class DeclarationApiClientSupport
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    internal static async Task EnsureSuccessAsync(
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
