using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class StatsRoutesIntegrationTests
{
    [Fact]
    public async Task Stats_Routes_ShouldReturnOverviewAndPopularTags()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        await TestAuthHelper.RegisterAndLoginAsync(client, "stats");

        var createTagResponse = await client.PostAsJsonAsync("/tags", new TagRequest($"stats-tag-{Guid.NewGuid():N}"));
        var createTagBody = await createTagResponse.Content.ReadAsStringAsync();
        Assert.True(createTagResponse.StatusCode == HttpStatusCode.Created, createTagBody);
        var createdTag = await createTagResponse.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(createdTag);

        var createCollectionResponse = await client.PostAsJsonAsync("/collections", new CollectionRequest($"stats-collection-{Guid.NewGuid():N}"));
        var createCollectionBody = await createCollectionResponse.Content.ReadAsStringAsync();
        Assert.True(createCollectionResponse.StatusCode == HttpStatusCode.Created, createCollectionBody);
        var createdCollection = await createCollectionResponse.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(createdCollection);

        var createLinkResponse = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            $"https://stats.example/{Guid.NewGuid():N}",
            "Stats Link",
            "For stats route validation",
            createdCollection!.Id,
            new[] { createdTag!.Id }));
        var createLinkBody = await createLinkResponse.Content.ReadAsStringAsync();
        Assert.True(createLinkResponse.StatusCode == HttpStatusCode.Created, createLinkBody);

        var overviewResponse = await client.GetAsync("/stats/overview");
        Assert.Equal(HttpStatusCode.OK, overviewResponse.StatusCode);
        var overview = await overviewResponse.Content.ReadFromJsonAsync<StatsOverviewResponse>();
        Assert.NotNull(overview);
        Assert.True(overview!.TotalLinks >= 1);
        Assert.True(overview.TotalTags >= 1);
        Assert.True(overview.TotalCollections >= 1);

        var popularTagsResponse = await client.GetAsync("/stats/popular-tags?limit=5");
        Assert.Equal(HttpStatusCode.OK, popularTagsResponse.StatusCode);
        var popularTags = await popularTagsResponse.Content.ReadFromJsonAsync<List<PopularTagResponse>>();
        Assert.NotNull(popularTags);
        Assert.Contains(popularTags!, tag => tag.Id == createdTag.Id);
    }

    private sealed record TagRequest(string Name);
    private sealed record TagResponse(Guid Id, string Name);
    private sealed record CollectionRequest(string Name);
    private sealed record CollectionResponse(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds);
    private sealed record StatsOverviewResponse(int TotalLinks, int FavoriteLinks, int TotalTags, int TotalCollections);
    private sealed record PopularTagResponse(Guid Id, string Name, int LinkCount);
}
