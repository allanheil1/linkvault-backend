using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LinkVault.IntegrationTests.Support;

internal static class TestAuthHelper
{
    public static async Task<AuthSession> RegisterAndLoginAsync(HttpClient client, string prefix, string password = "StrongPass123!")
    {
        var email = $"{prefix}-{Guid.NewGuid():N}@local.test";
        var registerResponse = await client.PostAsJsonAsync("/auth/register", new RegisterRequest($"{prefix} User", email, password));
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.StatusCode == HttpStatusCode.Created, registerBody);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        Assert.True(loginResponse.StatusCode == HttpStatusCode.OK, loginBody);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);
        Assert.False(string.IsNullOrWhiteSpace(loginPayload!.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        return new AuthSession(email, password, loginPayload.AccessToken, loginPayload.User.Id);
    }

    internal sealed record RegisterRequest(string Name, string Email, string Password);
    internal sealed record LoginRequest(string Email, string Password);
    internal sealed record LoginResponse(string AccessToken, AuthUser User);
    internal sealed record AuthUser(Guid Id, string Name, string Email);
    internal sealed record AuthSession(string Email, string Password, string AccessToken, Guid UserId);
}
