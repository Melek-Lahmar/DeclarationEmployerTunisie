using System.Net.Http;
using System.Text;
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
        var message = BuildFriendlyMessage((int)response.StatusCode, response.ReasonPhrase, content);
        throw new InvalidOperationException(message);
    }

    private static string BuildFriendlyMessage(int statusCode, string? reasonPhrase, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"Erreur API {statusCode} - {reasonPhrase}.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            var builder = new StringBuilder();

            if (root.TryGetProperty("error", out var error))
            {
                if (error.TryGetProperty("message", out var messageElement) && messageElement.ValueKind == JsonValueKind.String)
                {
                    builder.Append(messageElement.GetString());
                }

                if (error.TryGetProperty("details", out var detailsElement))
                {
                    var details = FlattenDetails(detailsElement).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                    if (details.Count > 0)
                    {
                        if (builder.Length > 0)
                        {
                            builder.AppendLine();
                        }

                        builder.Append(string.Join(Environment.NewLine, details));
                    }
                }
            }

            if (builder.Length > 0)
            {
                return builder.ToString();
            }
        }
        catch (JsonException)
        {
        }

        return $"Erreur API {statusCode} - {reasonPhrase} : {content}";
    }

    private static IEnumerable<string> FlattenDetails(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => [element.GetString() ?? string.Empty],
            JsonValueKind.Array => element.EnumerateArray().SelectMany(FlattenDetails),
            JsonValueKind.Object => element.EnumerateObject().SelectMany(property =>
                property.Value.ValueKind == JsonValueKind.Array
                    ? property.Value.EnumerateArray().Select(x => $"{property.Name} : {x.GetString()}")
                    : [$"{property.Name} : {property.Value}"]),
            _ => [element.ToString()]
        };
    }
}
