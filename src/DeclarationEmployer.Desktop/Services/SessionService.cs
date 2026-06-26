using DeclarationEmployer.Contracts.Users;

namespace DeclarationEmployer.Desktop.Services;

public sealed class SessionService
{
    public string? AccessToken { get; private set; }

    public UserDto? CurrentUser { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && CurrentUser is not null;

    public void SetSession(string accessToken, UserDto user)
    {
        AccessToken = accessToken;
        CurrentUser = user;
    }

    public void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
    }
}
