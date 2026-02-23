namespace LinkVault.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "linkvault";
    public string Audience { get; set; } = "linkvault";
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public string RefreshCookieName { get; set; } = "linkvault_refresh";
}
