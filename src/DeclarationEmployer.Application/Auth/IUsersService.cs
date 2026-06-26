using DeclarationEmployer.Contracts.Users;

namespace DeclarationEmployer.Application.Auth;

public interface IUsersService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto> CreateAsync(
        CreateUserRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<UserDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        Guid id,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
