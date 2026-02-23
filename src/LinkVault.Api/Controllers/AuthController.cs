using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LinkVault.Application.Features.Auth;
using LinkVault.Application.Features.Auth.Commands;
using LinkVault.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LinkVault.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(body.Name, body.Email, body.Password), ct);
        return CreatedAtAction(nameof(Me), new { }, result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var (response, refresh) = await _mediator.Send(new LoginCommand(body.Email, body.Password), ct);
        SetRefreshCookie(refresh.Token, refresh.ExpiresAt);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshCookieName = _configuration.GetSection("Jwt").GetValue<string>("RefreshCookieName") ?? "linkvault_refresh";
        var token = Request.Cookies[refreshCookieName];
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var (response, newRefresh) = await _mediator.Send(new RefreshCommand(token), ct);
        SetRefreshCookie(newRefresh.Token, newRefresh.ExpiresAt);
        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshCookieName = _configuration.GetSection("Jwt").GetValue<string>("RefreshCookieName") ?? "linkvault_refresh";
        var token = Request.Cookies[refreshCookieName];
        if (!string.IsNullOrWhiteSpace(token))
        {
            await _mediator.Send(new LogoutCommand(token), ct);
        }

        Response.Cookies.Delete(refreshCookieName, BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized();
        }

        var user = await _mediator.Send(new MeQuery(userId), ct);
        return user is null ? Unauthorized() : Ok(user);
    }

    private void SetRefreshCookie(string token, DateTimeOffset expires)
    {
        var options = BuildCookieOptions(expires);
        Response.Cookies.Append(GetRefreshCookieName(), token, options);
    }

    private string GetRefreshCookieName()
    {
        return _configuration.GetSection("Jwt").GetValue<string>("RefreshCookieName") ?? "linkvault_refresh";
    }

    private CookieOptions BuildCookieOptions(DateTimeOffset expires)
    {
        var sameSite = SameSiteMode.Strict;
        var secure = true;

        var env = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
        {
            // Allow SameSite=None only if running on localhost for dev tools.
            sameSite = SameSiteMode.None;
            secure = false;
        }

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = "/auth/refresh",
            Expires = expires.UtcDateTime,
        };
    }
}
