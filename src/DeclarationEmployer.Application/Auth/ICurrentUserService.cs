namespace DeclarationEmployer.Application.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? UserName { get; }

    string? Role { get; }

    bool IsAuthenticated { get; }
}
