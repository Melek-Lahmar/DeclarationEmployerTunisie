using DeclarationEmployer.Contracts.Cabinet;

namespace DeclarationEmployer.Application.Cabinet;

public interface IFiscalYearsService
{
    Task<IReadOnlyList<FiscalYearDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<FiscalYearDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FiscalYearDto>> GetByClientAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<FiscalYearDto> CreateAsync(
        Guid clientId,
        CreateFiscalYearRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<FiscalYearDto> UpdateAsync(
        Guid id,
        UpdateFiscalYearRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<FiscalYearDto> CloseAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<FiscalYearDto> ReopenAsync(
        Guid id,
        ReopenFiscalYearRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<FiscalYearStatusDto?> GetStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FiscalYearHistoryDto>> GetHistoryAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
