using DeclarationEmployer.Contracts.Auth;

namespace DeclarationEmployer.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
