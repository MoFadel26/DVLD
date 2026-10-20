using DVLD.Api.Auth;
using DVLD.Application.DTOs;
using DVLD.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DVLD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>Demo: signs in as a new temporary user with a random name. Limited per IP address.</summary>
    [AllowAnonymous]
    [EnableRateLimiting(GuestRateLimit.Policy)]
    [HttpPost("guest")]
    public async Task<ActionResult<LoginResponseDto>> Guest(CancellationToken cancellationToken)
    {
        var result = await _authService.SignInAsGuestAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(cancellationToken);
        return Ok(user);
    }
}
