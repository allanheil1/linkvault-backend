using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class LinksRoutesIntegrationTests
{
    [Fact]
    public async Task List_ShouldReturnUnauthorized_WhenMissingBearerToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/links");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedLink()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-create");

        var response = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            $"https://example.com/{Guid.NewGuid():N}",
            "Created Link",
            "Created from integration test",
            null,
            Array.Empty<Guid>()));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var payload = await response.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Created Link", payload!.Title);
    }

    [Fact]
    public async Task List_ShouldSupportQueryFilter()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-list");

        var marker = $"query-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync("/links", new CreateLinkRequest($"https://example.com/{marker}", marker, null, null, Array.Empty<Guid>()));
        await client.PostAsJsonAsync("/links", new CreateLinkRequest($"https://example.com/other-{Guid.NewGuid():N}", "Other title", null, null, Array.Empty<Guid>()));

        var response = await client.GetAsync($"/links?query={marker}&page=1&pageSize=10&sort=-createdAt");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<LinkResponse>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Items, x => x.Title == marker);
    }

    [Fact]
    public async Task GetById_ShouldReturnLink_WhenItExists()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-get");

        var created = await CreateLinkAsync(client, "GetById title");
        var response = await client.GetAsync($"/links/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(payload);
        Assert.Equal(created.Id, payload!.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenLinkDoesNotExist()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-missing");

        var response = await client.GetAsync($"/links/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnUpdatedLink()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-update");

        var created = await CreateLinkAsync(client, "Before update");

        var response = await client.PutAsJsonAsync($"/links/{created.Id}", new UpdateLinkRequest(
            created.Url,
            "After update",
            "Updated note",
            null,
            Array.Empty<Guid>(),
            true));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);

        var payload = await response.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(payload);
        Assert.Equal("After update", payload!.Title);
        Assert.True(payload.IsFavorite);
    }

    [Fact]
    public async Task ToggleFavorite_ShouldInvertFavoriteFlag()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-favorite");

        var created = await CreateLinkAsync(client, "Favorite test");

        var response = await client.PatchAsync($"/links/{created.Id}/favorite", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsFavorite);
    }

    [Fact]
    public async Task Delete_ShouldRemoveLink()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "links-delete");

        var created = await CreateLinkAsync(client, "Delete test");

        var deleteResponse = await client.DeleteAsync($"/links/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/links/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static async Task<LinkResponse> CreateLinkAsync(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            $"https://example.com/{Guid.NewGuid():N}",
            title,
            null,
            null,
            Array.Empty<Guid>()));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var payload = await response.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(payload);
        return payload!;
    }

    private sealed record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds);
    private sealed record UpdateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds, bool IsFavorite);
    private sealed record LinkResponse(Guid Id, string Url, string Title, string? Note, Guid? CollectionId, bool IsFavorite);
    private sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
}
