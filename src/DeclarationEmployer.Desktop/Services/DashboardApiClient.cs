using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DeclarationEmployer.Contracts.Dashboard;

namespace DeclarationEmployer.Desktop.Services;

public sealed class DashboardApiClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DashboardSummaryDto>(
            "api/dashboard/summary",
            JsonOptions,
            cancellationToken) ?? new DashboardSummaryDto();
    }

    public async Task<IReadOnlyList<RecentActionDto>> GetRecentActionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<List<RecentActionDto>>(
            "api/dashboard/recent-actions?count=10",
            JsonOptions,
            cancellationToken) ?? [];
    }

    public async Task<DeclarationProgressDto> GetProgressAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DeclarationProgressDto>(
            "api/dashboard/progress",
            JsonOptions,
            cancellationToken) ?? new DeclarationProgressDto();
    }
}
