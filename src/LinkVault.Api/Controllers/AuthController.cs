using LinkVault.Api.Controllers.Extensions;
using LinkVault.Application.Features.Auth;
using LinkVault.Application.Features.Auth.Commands;
using LinkVault.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LinkVault.Api.Controllers;

/// <summary>
/// Authentication endpoints for user registration, login, token refresh, and session management.
/// </summary>
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

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="body">User registration payload.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The created user profile.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(body.Name, body.Email, body.Password), ct);
        return CreatedAtAction(nameof(Me), new { }, result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <remarks>
    /// Also sets a refresh token cookie used by <c>POST /auth/refresh</c>.
    /// </remarks>
    /// <param name="body">User credentials.</param>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>Access token plus basic user info.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var (response, refresh) = await _mediator.Send(new LoginCommand(body.Email, body.Password), ct);
        SetRefreshCookie(refresh.Token, refresh.ExpiresAt);
        return Ok(response);
    }

    /// <summary>
    /// Rotates refresh token and issues a new access token.
    /// </summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>A fresh access token.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Logs out the current session by revoking and clearing the refresh token cookie.
    /// </summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>No content.</returns>
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

    /// <summary>
    /// Returns the currently authenticated user profile.
    /// </summary>
    /// <param name="ct">Request cancellation token.</param>
    /// <returns>The authenticated user profile.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _mediator.Send(new MeQuery(userId.Value), ct);
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
            // Use Lax in development to avoid browsers rejecting insecure SameSite=None cookies.
            sameSite = SameSiteMode.Lax;
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
