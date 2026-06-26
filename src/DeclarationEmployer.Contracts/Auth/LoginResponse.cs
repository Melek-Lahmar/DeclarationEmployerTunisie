using DeclarationEmployer.Contracts.Users;

namespace DeclarationEmployer.Contracts.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public UserDto User { get; set; } = new();

    public DateTimeOffset ExpiresAt { get; set; }
}
