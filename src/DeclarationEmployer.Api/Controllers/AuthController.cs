using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Contracts.Auth;
using DeclarationEmployer.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeclarationEmployer.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsersService _usersService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        IAuthService authService,
        IUsersService usersService,
        ICurrentUserService currentUserService)
    {
        _authService = authService;
        _usersService = usersService;
        _currentUserService = currentUserService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _authService.LoginAsync(request, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var user = await _usersService.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "Utilisateur introuvable." });
        }

        return Ok(user);
    }
}
