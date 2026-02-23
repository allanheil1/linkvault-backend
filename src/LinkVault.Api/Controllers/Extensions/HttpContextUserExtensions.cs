using System.IdentityModel.Tokens.Jwt;

namespace LinkVault.Api.Controllers.Extensions;

public static class HttpContextUserExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? context.User.FindFirst("sub")?.Value;
        if (Guid.TryParse(sub, out var userId))
            return userId;
        return null;
    }
}
