using System.Net.Http;
using System.Net.Http.Json;
using DeclarationEmployer.Contracts.Declarations.Empcca;

namespace DeclarationEmployer.Desktop.Services;

public sealed class EmpccaAnnexesApiClient
{
    private readonly HttpClient _httpClient;

    public EmpccaAnnexesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IReadOnlyList<EmpccaAnnexA1LineDto>> GetA1LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaAnnexA1LineDto>>($"api/declarations/{declarationId}/empcca/annexes/A1/lines", cancellationToken);

    public Task<EmpccaAnnexA1LineDto> CreateA1LineAsync(Guid declarationId, CreateEmpccaAnnexA1LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexA1LineDto>($"api/declarations/{declarationId}/empcca/annexes/A1/lines", request, cancellationToken);

    public Task<EmpccaAnnexA1LineDto> UpdateA1LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA1LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaAnnexA1LineDto>($"api/declarations/{declarationId}/empcca/annexes/A1/lines/{lineId}", request, cancellationToken);

    public Task DeleteA1LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A1/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexA1SummaryDto> GetA1SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexA1SummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A1/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA1Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A1/validate", new { }, cancellationToken);

    public Task<IReadOnlyList<EmpccaAnnexA2LineDto>> GetA2LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaAnnexA2LineDto>>($"api/declarations/{declarationId}/empcca/annexes/A2/lines", cancellationToken);

    public Task<EmpccaAnnexA2LineDto> CreateA2LineAsync(Guid declarationId, CreateEmpccaAnnexA2LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexA2LineDto>($"api/declarations/{declarationId}/empcca/annexes/A2/lines", request, cancellationToken);

    public Task<EmpccaAnnexA2LineDto> UpdateA2LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA2LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaAnnexA2LineDto>($"api/declarations/{declarationId}/empcca/annexes/A2/lines/{lineId}", request, cancellationToken);

    public Task DeleteA2LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A2/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexA2SummaryDto> GetA2SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexA2SummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A2/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA2Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A2/validate", new { }, cancellationToken);

    public Task<IReadOnlyList<EmpccaAnnexA5LineDto>> GetA5LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaAnnexA5LineDto>>($"api/declarations/{declarationId}/empcca/annexes/A5/lines", cancellationToken);

    public Task<EmpccaAnnexA5LineDto> CreateA5LineAsync(Guid declarationId, CreateEmpccaAnnexA5LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexA5LineDto>($"api/declarations/{declarationId}/empcca/annexes/A5/lines", request, cancellationToken);

    public Task<EmpccaAnnexA5LineDto> UpdateA5LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA5LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaAnnexA5LineDto>($"api/declarations/{declarationId}/empcca/annexes/A5/lines/{lineId}", request, cancellationToken);

    public Task DeleteA5LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A5/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexA5SummaryDto> GetA5SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexA5SummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A5/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA5Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A5/validate", new { }, cancellationToken);

    public Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>>> GetA3LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>>>($"api/declarations/{declarationId}/empcca/annexes/A3/lines", cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>> CreateA3LineAsync(Guid declarationId, CreateEmpccaAnnexA3LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A3/lines", request, cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>> UpdateA3LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA3LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A3/lines/{lineId}", request, cancellationToken);

    public Task DeleteA3LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A3/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexSummaryDto> GetA3SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexSummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A3/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA3Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A3/validate", new { }, cancellationToken);

    public Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>>> GetA4LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>>>($"api/declarations/{declarationId}/empcca/annexes/A4/lines", cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>> CreateA4LineAsync(Guid declarationId, CreateEmpccaAnnexA4LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A4/lines", request, cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>> UpdateA4LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA4LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A4/lines/{lineId}", request, cancellationToken);

    public Task DeleteA4LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A4/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexSummaryDto> GetA4SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexSummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A4/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA4Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A4/validate", new { }, cancellationToken);

    public Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>>> GetA6LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>>>($"api/declarations/{declarationId}/empcca/annexes/A6/lines", cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>> CreateA6LineAsync(Guid declarationId, CreateEmpccaAnnexA6LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A6/lines", request, cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>> UpdateA6LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA6LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A6/lines/{lineId}", request, cancellationToken);

    public Task DeleteA6LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A6/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexSummaryDto> GetA6SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexSummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A6/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA6Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A6/validate", new { }, cancellationToken);

    public Task<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>>> GetA7LinesAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>>>($"api/declarations/{declarationId}/empcca/annexes/A7/lines", cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>> CreateA7LineAsync(Guid declarationId, CreateEmpccaAnnexA7LineRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A7/lines", request, cancellationToken);

    public Task<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>> UpdateA7LineAsync(Guid declarationId, Guid lineId, CreateEmpccaAnnexA7LineRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest>>($"api/declarations/{declarationId}/empcca/annexes/A7/lines/{lineId}", request, cancellationToken);

    public Task DeleteA7LineAsync(Guid declarationId, Guid lineId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/declarations/{declarationId}/empcca/annexes/A7/lines/{lineId}", cancellationToken);

    public Task<EmpccaAnnexSummaryDto> GetA7SummaryAsync(Guid declarationId, CancellationToken cancellationToken = default) =>
        GetAsync<EmpccaAnnexSummaryDto>($"api/declarations/{declarationId}/empcca/annexes/A7/summary", cancellationToken);

    public Task<EmpccaAnnexValidationDto> ValidateA7Async(Guid declarationId, CancellationToken cancellationToken = default) =>
        PostAsync<EmpccaAnnexValidationDto>($"api/declarations/{declarationId}/empcca/annexes/A7/validate", new { }, cancellationToken);

    private async Task<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetFromJsonAsync<T>(uri, DeclarationApiClientSupport.JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Reponse API vide.");
    }

    private async Task<T> PostAsync<T>(string uri, object request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(uri, request, DeclarationApiClientSupport.JsonOptions, cancellationToken);
        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<T>(DeclarationApiClientSupport.JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Reponse API vide.");
    }

    private async Task<T> PutAsync<T>(string uri, object request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync(uri, request, DeclarationApiClientSupport.JsonOptions, cancellationToken);
        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<T>(DeclarationApiClientSupport.JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Reponse API vide.");
    }

    private async Task DeleteAsync(string uri, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync(uri, cancellationToken);
        await DeclarationApiClientSupport.EnsureSuccessAsync(response, cancellationToken);
    }
}
