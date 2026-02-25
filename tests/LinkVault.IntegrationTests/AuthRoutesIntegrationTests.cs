using System.Net;
using System.Net.Http.Json;

namespace LinkVault.IntegrationTests;

public class AuthRoutesIntegrationTests
{
    [Fact]
    public async Task Auth_Routes_ShouldSupportFullSessionLifecycle()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var email = $"auth-{Guid.NewGuid():N}@local.test";
        const string password = "StrongPass123!";

        var registerResponse = await client.PostAsJsonAsync("/auth/register", new RegisterRequest("Auth User", email, password));
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.StatusCode == HttpStatusCode.Created, registerBody);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthUserResponse>();
        Assert.NotNull(registered);
        Assert.Equal(email, registered!.Email);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        Assert.True(loginResponse.StatusCode == HttpStatusCode.OK, loginBody);
        Assert.Contains(loginResponse.Headers, h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase));
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);
        Assert.False(string.IsNullOrWhiteSpace(loginPayload!.AccessToken));

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var meResponse = await client.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<AuthUserResponse>();
        Assert.NotNull(me);
        Assert.Equal(email, me!.Email);

        var refreshResponse = await client.PostAsync("/auth/refresh", null);
        var refreshBody = await refreshResponse.Content.ReadAsStringAsync();
        Assert.True(refreshResponse.StatusCode == HttpStatusCode.OK, refreshBody);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<RefreshResponse>();
        Assert.NotNull(refreshed);
        Assert.False(string.IsNullOrWhiteSpace(refreshed!.AccessToken));

        var logoutResponse = await client.PostAsync("/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogoutResponse = await client.PostAsync("/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogoutResponse.StatusCode);
    }

    private sealed record RegisterRequest(string Name, string Email, string Password);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record AuthUserResponse(Guid Id, string Name, string Email);
    private sealed record LoginResponse(string AccessToken, AuthUserResponse User);
    private sealed record RefreshResponse(string AccessToken);
}
