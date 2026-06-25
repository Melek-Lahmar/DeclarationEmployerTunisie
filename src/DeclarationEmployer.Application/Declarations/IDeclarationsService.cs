using DeclarationEmployer.Contracts.Declarations;

namespace DeclarationEmployer.Application.Declarations;

public interface IDeclarationsService
{
    Task<IReadOnlyList<DeclarationDto>> GetAllAsync(
        Guid? clientId,
        Guid? fiscalYearId,
        string? status,
        CancellationToken cancellationToken = default);

    Task<DeclarationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DeclarationDto> CreateAsync(
        CreateDeclarationRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<DeclarationDto> UpdateAsync(
        Guid id,
        UpdateDeclarationRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<DeclarationSummaryDto?> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeclarationEventDto>> GetEventsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DeclarationDto> LockAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<DeclarationDto> CloseAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
