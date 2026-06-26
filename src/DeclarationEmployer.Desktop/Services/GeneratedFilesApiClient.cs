using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Desktop.Services;

public sealed class GeneratedFilesApiClient
{
    private readonly HttpClient _httpClient;

    public GeneratedFilesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<GeneratedFileDto>> GetByDeclarationAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<GeneratedFileDto>>(
            $"api/declarations/{declarationId}/generated-files",
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? [];
    }
}
