using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class TagsRoutesIntegrationTests
{
    [Fact]
    public async Task List_ShouldReturnUnauthorized_WhenMissingBearerToken()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/tags");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedTag()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "tags-create");

        var name = $"tag-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/tags", new TagRequest(name));
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var payload = await response.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(payload);
        Assert.Equal(name, payload!.Name);
    }

    [Fact]
    public async Task List_ShouldReturnCreatedTag()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "tags-list");

        var created = await CreateTagAsync(client);

        var response = await client.GetAsync("/tags");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<TagResponse>>();
        Assert.NotNull(payload);
        Assert.Contains(payload!, tag => tag.Id == created.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnTag_WhenItExists()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "tags-get");

        var created = await CreateTagAsync(client);

        var response = await client.GetAsync($"/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(payload);
        Assert.Equal(created.Id, payload!.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenTagDoesNotExist()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "tags-missing");

        var response = await client.GetAsync($"/tags/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturnUpdatedTag()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "tags-update");

        var created = await CreateTagAsync(client);
        var updatedName = $"updated-{Guid.NewGuid():N}";

        var response = await client.PutAsJsonAsync($"/tags/{created.Id}", new TagRequest(updatedName));
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);

        var payload = await response.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(payload);
        Assert.Equal(updatedName, payload!.Name);
    }

    [Fact]
    public async Task Delete_ShouldRemoveTag()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await TestAuthHelper.RegisterAndLoginAsync(client, "tags-delete");

        var created = await CreateTagAsync(client);

        var deleteResponse = await client.DeleteAsync($"/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static async Task<TagResponse> CreateTagAsync(HttpClient client)
    {
        var name = $"tag-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/tags", new TagRequest(name));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, body);

        var payload = await response.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(payload);
        return payload!;
    }

    private sealed record TagRequest(string Name);
    private sealed record TagResponse(Guid Id, string Name);
}
