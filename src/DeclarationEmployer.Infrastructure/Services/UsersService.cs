using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Users;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class UsersService : IUsersService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHostEnvironment _environment;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public UsersService(
        ApplicationDbContext db,
        IPasswordService passwordService,
        ICurrentUserService currentUserService,
        IHostEnvironment environment,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _db = db;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
        _environment = environment;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .OrderBy(x => x.UserName)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDto> CreateAsync(
        CreateUserRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedUserName = request.UserName.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var role = ParseRole(request.Role);

        await EnsureUniqueAsync(normalizedUserName, normalizedEmail, null, cancellationToken);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedUserName,
            Email = normalizedEmail,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = role,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        AddAudit("USER_CREATED", user.Id, $"Creation utilisateur : {user.UserName} ({user.Role})", ipAddress);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            throw new ApplicationNotFoundException("Utilisateur introuvable.");
        }

        var normalizedUserName = request.UserName.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        await EnsureUniqueAsync(normalizedUserName, normalizedEmail, id, cancellationToken);

        user.UserName = normalizedUserName;
        user.Email = normalizedEmail;
        user.Role = ParseRole(request.Role);
        user.IsActive = request.IsActive;

        AddAudit("USER_UPDATED", user.Id, $"Modification utilisateur : {user.UserName} ({user.Role})", ipAddress);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task ChangePasswordAsync(
        Guid id,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            throw new ApplicationNotFoundException("Utilisateur introuvable.");
        }

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new ApplicationConflictException("Le mot de passe actuel est invalide.");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        AddAudit("USER_PASSWORD_CHANGED", user.Id, $"Changement mot de passe utilisateur : {user.UserName}", ipAddress);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(
        Guid id,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            throw new ApplicationNotFoundException("Utilisateur introuvable.");
        }

        user.IsActive = false;
        AddAudit("USER_DEACTIVATED", user.Id, $"Desactivation utilisateur : {user.UserName}", ipAddress);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueAsync(
        string userName,
        string email,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        var duplicateUserName = await _db.Users
            .AnyAsync(x => x.UserName.ToLower() == userName.ToLower() && x.Id != currentId, cancellationToken);
        if (duplicateUserName)
        {
            throw new ApplicationConflictException("Un utilisateur avec ce nom existe deja.");
        }

        var duplicateEmail = await _db.Users
            .AnyAsync(x => x.Email.ToLower() == email && x.Id != currentId, cancellationToken);
        if (duplicateEmail)
        {
            throw new ApplicationConflictException("Un utilisateur avec cet email existe deja.");
        }
    }

    private static UserRole ParseRole(string role)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            throw new ApplicationConflictException("Role utilisateur invalide.");
        }

        return parsedRole;
    }

    private UserDto ToDto(ApplicationUser user)
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

    private void AddAudit(string action, Guid userId, string details, string? ipAddress)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityName = nameof(ApplicationUser),
            EntityId = userId.ToString(),
            UserName = GetAuditUserName(),
            Details = details,
            IpAddress = ipAddress,
            OccurredAt = DateTimeOffset.UtcNow
        });
    }

    private string GetAuditUserName()
    {
        if (_currentUserService.IsAuthenticated && !string.IsNullOrWhiteSpace(_currentUserService.UserName))
        {
            return _currentUserService.UserName!;
        }

        return _environment.IsDevelopment() ? "system-dev" : "system";
    }
}
