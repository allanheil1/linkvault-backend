using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class CollectionsRoutesIntegrationTests
{
    [Fact]
    public async Task Collections_Routes_ShouldSupportFullCrudAndCollectionLinksListing()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        await TestAuthHelper.RegisterAndLoginAsync(client, "collections");

        var createCollectionResponse = await client.PostAsJsonAsync("/collections", new CollectionRequest($"collection-{Guid.NewGuid():N}"));
        var createCollectionBody = await createCollectionResponse.Content.ReadAsStringAsync();
        Assert.True(createCollectionResponse.StatusCode == HttpStatusCode.Created, createCollectionBody);
        var createdCollection = await createCollectionResponse.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(createdCollection);

        var listCollectionsResponse = await client.GetAsync("/collections");
        Assert.Equal(HttpStatusCode.OK, listCollectionsResponse.StatusCode);
        var listedCollections = await listCollectionsResponse.Content.ReadFromJsonAsync<List<CollectionResponse>>();
        Assert.NotNull(listedCollections);
        Assert.Contains(listedCollections!, collection => collection.Id == createdCollection!.Id);

        var getByIdResponse = await client.GetAsync($"/collections/{createdCollection!.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var listLinksBeforeCreateResponse = await client.GetAsync($"/collections/{createdCollection.Id}/links?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listLinksBeforeCreateResponse.StatusCode);

        var createLinkResponse = await client.PostAsJsonAsync("/links", new CreateLinkRequest(
            $"https://collection.example/{Guid.NewGuid():N}",
            "Collection Link",
            null,
            createdCollection.Id,
            Array.Empty<Guid>()));
        var createLinkBody = await createLinkResponse.Content.ReadAsStringAsync();
        Assert.True(createLinkResponse.StatusCode == HttpStatusCode.Created, createLinkBody);
        var createdLink = await createLinkResponse.Content.ReadFromJsonAsync<LinkResponse>();
        Assert.NotNull(createdLink);

        var listLinksAfterCreateResponse = await client.GetAsync($"/collections/{createdCollection.Id}/links?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listLinksAfterCreateResponse.StatusCode);
        var listedLinks = await listLinksAfterCreateResponse.Content.ReadFromJsonAsync<PagedResult<LinkResponse>>();
        Assert.NotNull(listedLinks);
        Assert.Contains(listedLinks!.Items, link => link.Id == createdLink!.Id);

        var updatedName = $"updated-collection-{Guid.NewGuid():N}";
        var updateCollectionResponse = await client.PutAsJsonAsync($"/collections/{createdCollection.Id}", new CollectionRequest(updatedName));
        var updateCollectionBody = await updateCollectionResponse.Content.ReadAsStringAsync();
        Assert.True(updateCollectionResponse.StatusCode == HttpStatusCode.OK, updateCollectionBody);
        var updatedCollection = await updateCollectionResponse.Content.ReadFromJsonAsync<CollectionResponse>();
        Assert.NotNull(updatedCollection);
        Assert.Equal(updatedName, updatedCollection!.Name);

        var deleteCollectionResponse = await client.DeleteAsync($"/collections/{createdCollection.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteCollectionResponse.StatusCode);

        var getAfterDeleteResponse = await client.GetAsync($"/collections/{createdCollection.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    private sealed record CollectionRequest(string Name);
    private sealed record CollectionResponse(Guid Id, string Name, DateTimeOffset CreatedAt);
    private sealed record CreateLinkRequest(string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds);
    private sealed record LinkResponse(Guid Id, string Url, string Title, string? Note, Guid? CollectionId, bool IsFavorite);
    private sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
}
