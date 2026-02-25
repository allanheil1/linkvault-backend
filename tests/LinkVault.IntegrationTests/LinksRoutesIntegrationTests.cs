using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class LinksRoutesIntegrationTests
{
    [Fact]
    public async Task Links_Routes_ShouldSupportFullCrudAndFavoriteToggle()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        await TestAuthHelper.RegisterAndLoginAsync(client, "links");

        var marker = $"links-{Guid.NewGuid():N}";
        var createResponse = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            $"https://example.com/{marker}",
            $"Title {marker}",
            "Initial note",
            null,
            Array.Empty<Guid>()));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.True(createResponse.StatusCode == HttpStatusCode.Created, createBody);
        var created = await createResponse.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(created);

        var listResponse = await client.GetAsync($"/links?query={marker}&page=1&pageSize=10&sort=-createdAt");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listed = await listResponse.Content.ReadFromJsonAsync<PagedResult<LinkResponse>>();
        Assert.NotNull(listed);
        Assert.Contains(listed!.Items, item => item.Id == created!.Id);

        var getByIdResponse = await client.GetAsync($"/links/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/links/{created.Id}", new UpdateLinkRequest(
            $"https://example.com/{marker}/updated",
            $"Updated {marker}",
            "Updated note",
            null,
            Array.Empty<Guid>(),
            true));
        var updateBody = await updateResponse.Content.ReadAsStringAsync();
        Assert.True(updateResponse.StatusCode == HttpStatusCode.OK, updateBody);

        var toggleFavoriteResponse = await client.PatchAsync($"/links/{created.Id}/favorite", null);
        Assert.Equal(HttpStatusCode.OK, toggleFavoriteResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/links/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await client.GetAsync($"/links/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    private sealed record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds);
    private sealed record UpdateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds, bool IsFavorite);
    private sealed record LinkResponse(Guid Id, string Url, string Title, string? Note, Guid? CollectionId, bool IsFavorite);
    private sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
}
