using DeclarationEmployer.Domain.Auth;

namespace DeclarationEmployer.Application.Auth;

public interface IJwtTokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) GenerateToken(ApplicationUser user);
}
