using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Contracts.Common;

namespace DeclarationEmployer.Application.Cabinet;

public interface IClientsService
{
    Task<IReadOnlyList<ClientCompanyDto>> GetAllAsync(
        bool includeInactive,
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ClientCompanyDto>> GetPagedAsync(
        bool includeInactive,
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ClientCompanyDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ClientSummaryDto?> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientHistoryDto>> GetHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ClientCompanyDto> CreateAsync(
        CreateClientCompanyRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<ClientCompanyDto> UpdateAsync(
        Guid id,
        UpdateClientCompanyRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
