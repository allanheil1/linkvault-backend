using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LinkVault.Api.Controllers.Extensions;

public static class HttpContextUserExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(sub, out var userId))
            return userId;

        return null;
    }
}
