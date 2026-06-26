using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Auth;
using DeclarationEmployer.Contracts.Users;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<LoginRequest> _validator;

    public AuthService(
        ApplicationDbContext db,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IValidator<LoginRequest> validator)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _validator = validator;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var normalized = request.UserNameOrEmail.Trim().ToLowerInvariant();
        var user = await _db.Users
            .FirstOrDefaultAsync(
                x => x.UserName.ToLower() == normalized || x.Email.ToLower() == normalized,
                cancellationToken);

        if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new ApplicationUnauthorizedException("Identifiants invalides.");
        }

        if (!user.IsActive)
        {
            throw new ApplicationUnauthorizedException("Cet utilisateur est desactive.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = ToDto(user)
        };
    }

    private static UserDto ToDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
