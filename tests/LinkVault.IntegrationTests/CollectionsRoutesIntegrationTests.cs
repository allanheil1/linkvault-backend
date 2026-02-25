using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class CollectionsRoutesIntegrationTests
{
    [Fact]
    public async Task List_ShouldReturnUnauthorized_WhenMissingBearerToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/collections");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Links_ShouldReturnUnauthorized_WhenMissingBearerToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/collections/{Guid.NewGuid()}/links?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedCollection()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-create");

        var name = $"collection-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/collections", new CollectionRequest(name));
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var payload = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(payload);
        Assert.Equal(name, payload!.Name);
    }

    [Fact]
    public async Task List_ShouldReturnCreatedCollection()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-list");

        var created = await CreateCollectionAsync(client);

        var response = await client.GetAsync("/collections");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<CollectionResponse>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!, collection => collection.Id == created.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnCollection_WhenItExists()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-get");

        var created = await CreateCollectionAsync(client);

        var response = await client.GetAsync($"/collections/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(payload);
        Assert.Equal(created.Id, payload!.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCollectionDoesNotExist()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-missing");

        var response = await client.GetAsync($"/collections/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnUpdatedCollection()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-update");

        var created = await CreateCollectionAsync(client);
        var updatedName = $"updated-collection-{Guid.NewGuid():N}";

        var response = await client.PutAsJsonAsync($"/collections/{created.Id}", new CollectionRequest(updatedName));
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);

        var payload = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(payload);
        Assert.Equal(updatedName, payload!.Name);
    }

    [Fact]
    public async Task Links_ShouldReturnPagedLinksFromCollection()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-links");

        var collection = await CreateCollectionAsync(client);

        var linkCreateResponse = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            $"https://collection.example/{Guid.NewGuid():N}",
            "Collection Link",
            null,
            collection.Id,
            Array.Empty<Guid>()));
        var linkCreateBody = await linkCreateResponse.Content.ReadAsStringAsync();
        Assert.True(linkCreateResponse.StatusCode == HttpStatusCode.Created, linkCreateBody);
        var createdLink = await linkCreateResponse.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(createdLink);

        var response = await client.GetAsync($"/collections/{collection.Id}/links?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<LinkResponse>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Items, link => link.Id == createdLink!.Id);
    }

    [Fact]
    public async Task Links_ShouldReturnNotFound_WhenCollectionDoesNotExist()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-links-missing");

        var response = await client.GetAsync($"/collections/{Guid.NewGuid()}/links?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldRemoveCollection()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "collections-delete");

        var created = await CreateCollectionAsync(client);

        var deleteResponse = await client.DeleteAsync($"/collections/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/collections/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static async Task<CollectionResponse> CreateCollectionAsync(HttpClient client)
    {
        var name = $"collection-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/collections", new CollectionRequest(name));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var payload = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(payload);
        return payload!;
    }

    private sealed record CollectionRequest(string Name);
    private sealed record CollectionResponse(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds);
    private sealed record LinkResponse(Guid Id, string Url, string Title, string? Note, Guid? CollectionId, bool IsFavorite);
    private sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
}
