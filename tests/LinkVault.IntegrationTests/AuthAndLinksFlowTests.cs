using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LinkVault.IntegrationTests;

public class AuthAndLinksFlowTests
{

    [Fact]
    public async Task Register_Then_Login_ShouldReturnAccessToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var register = new RegisterRequest(
            "Integration User",
            $"int-{Guid.NewGuid():N}@local.test",
            "StrongPass123!");

        var registerResponse = await client.PostAsJsonAsync("/auth/register", register);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.StatusCode == HttpStatusCode.Created, registerBody);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest(register.Email, register.Password));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var payload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.AccessToken));
    }

    [Fact]
    public async Task CreateLink_ThenListWithQueryFilter_ShouldReturnCreatedLink()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var register = new RegisterRequest(
            "Flow User",
            $"flow-{Guid.NewGuid():N}@local.test",
            "StrongPass123!");

        var registerResponse = await client.PostAsJsonAsync("/auth/register", register);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(registerResponse.StatusCode == HttpStatusCode.Created, registerBody);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginRequest(register.Email, register.Password));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload!.AccessToken);

        var createResponse = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            "https://example.com/docs",
            "Example Docs",
            "Integration test note",
            null,
            Array.Empty<Guid>()));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await client.GetAsync("/links?query=Example&page=1&pageSize=10&sort=-createdAt");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PagedResult<LinkResponse>>();
        Assert.NotNull(listPayload);
        Assert.True(listPayload!.Total >= 1);
        Assert.Contains(listPayload.Items, item => item.Title == "Example Docs");
    }

    private sealed record RegisterRequest(string Name, string Email, string Password);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record LoginResponse(string AccessToken, AuthUser User);
    private sealed record AuthUser(Guid Id, string Name, string Email);
    private sealed record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds);
    private sealed record LinkResponse(Guid Id, string Url, string Title);
    private sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
}
