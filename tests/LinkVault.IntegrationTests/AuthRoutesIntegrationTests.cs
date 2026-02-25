using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LinkVault.IntegrationTests;

public class AuthRoutesIntegrationTests
{
    [Fact]
    public async Task Register_ShouldReturnCreatedUser()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var email = $"register-{Guid.NewGuid():N}@local.test";
        var response = await client.PostAsJsonAsync("/auth/register", new RegisterRequest("Register User", email, "StrongPass123!"));

        var payload = await response.Content.ReadFromJsonAsync<AuthUserResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(email, payload!.Email);
    }

    [Fact]
    public async Task Login_ShouldReturnAccessTokenAndRefreshCookie()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var credentials = await RegisterUserAsync(client, "login-success");

        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest(credentials.Email, credentials.Password));
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains(response.Headers, header => header.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase));

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var credentials = await RegisterUserAsync(client, "login-fail");

        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest(credentials.Email, "WrongPassword123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnUnauthorized_WhenMissingBearerToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnCurrentUser_WhenAuthenticated()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var credentials = await RegisterUserAsync(client, "me");
        var login = await LoginAsync(client, credentials);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await client.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AuthUserResponse>();
        Assert.NotNull(payload);
        Assert.Equal(credentials.Email, payload!.Email);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshCookieIsMissing()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewAccessToken_WhenRefreshCookieIsValid()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var credentials = await RegisterUserAsync(client, "refresh");
        await LoginAsync(client, credentials);

        var response = await client.PostAsync("/auth/refresh", null);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);

        var payload = await response.Content.ReadFromJsonAsync<RefreshResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task Logout_ShouldInvalidateRefreshToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var credentials = await RegisterUserAsync(client, "logout");
        await LoginAsync(client, credentials);

        var logoutResponse = await client.PostAsync("/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogout = await client.PostAsync("/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    private static async Task<TestCredentials> RegisterUserAsync(HttpClient client, string prefix)
    {
        var email = $"{prefix}-{Guid.NewGuid():N}@local.test";
        const string password = "StrongPass123!";

        var response = await client.PostAsJsonAsync("/auth/register", new RegisterRequest($"{prefix} user", email, password));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        return new TestCredentials(email, password);
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client, TestCredentials credentials)
    {
        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest(credentials.Email, credentials.Password));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(payload);
        return payload!;
    }

    private sealed record RegisterRequest(string Name, string Email, string Password);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record AuthUserResponse(Guid Id, string Name, string Email);
    private sealed record LoginResponse(string AccessToken, AuthUserResponse User);
    private sealed record RefreshResponse(string AccessToken);
    private sealed record TestCredentials(string Email, string Password);
}
