using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DevelopmentAdminSeedService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly IOptions<DefaultAdminOptions> _options;
    private readonly IHostEnvironment _environment;

    public DevelopmentAdminSeedService(
        ApplicationDbContext db,
        IPasswordService passwordService,
        IOptions<DefaultAdminOptions> options,
        IHostEnvironment environment)
    {
        _db = db;
        _passwordService = passwordService;
        _options = options;
        _environment = environment;
    }

    public async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        if (await _db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var options = _options.Value;
        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException("DefaultAdmin:Password doit etre configure localement en Developpement.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = options.UserName.Trim(),
            Email = options.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordService.HashPassword(options.Password),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
