using System.Net.Http;
using System.Net.Http.Json;
using System.IO;
using DeclarationEmployer.Contracts.Import;

namespace DeclarationEmployer.Desktop.Services;

public sealed class ImportExcelApiClient
{
    private readonly HttpClient _httpClient;

    public ImportExcelApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExcelImportPreviewDto> PreviewAsync(
        Guid declarationId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(filePath);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        var response = await _httpClient.PostAsync(
            $"api/declarations/{declarationId}/import/excel/preview",
            content,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ExcelImportPreviewDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres previsualisation.");
    }

    public async Task<ExcelImportCommitResultDto> CommitAsync(
        Guid declarationId,
        ExcelImportCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/declarations/{declarationId}/import/excel/commit",
            request,
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ExcelImportCommitResultDto>(
            DeclarationApiClientSupport.JsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Reponse API vide apres import.");
    }
}
