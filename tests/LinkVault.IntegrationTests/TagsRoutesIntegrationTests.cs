using System.Net;
using System.Net.Http.Json;
using LinkVault.IntegrationTests.Support;

namespace LinkVault.IntegrationTests;

public class TagsRoutesIntegrationTests
{
    [Fact]
    public async Task Tags_Routes_ShouldSupportFullCrud()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        await TestAuthHelper.RegisterAndLoginAsync(client, "tags");

        var createResponse = await client.PostAsJsonAsync("/tags", new TagRequest($"tag-{Guid.NewGuid():N}"));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.True(createResponse.StatusCode == HttpStatusCode.Created, createBody);
        var created = await createResponse.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(created);

        var listResponse = await client.GetAsync("/tags");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listed = await listResponse.Content.ReadFromJsonAsync<List<TagResponse>>();
        Assert.NotNull(listed);
        Assert.Contains(listed!, tag => tag.Id == created!.Id);

        var getByIdResponse = await client.GetAsync($"/tags/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var updatedName = $"updated-{Guid.NewGuid():N}";
        var updateResponse = await client.PutAsJsonAsync($"/tags/{created.Id}", new TagRequest(updatedName));
        var updateBody = await updateResponse.Content.ReadAsStringAsync();
        Assert.True(updateResponse.StatusCode == HttpStatusCode.OK, updateBody);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TagResponse>();
        Assert.NotNull(updated);
        Assert.Equal(updatedName, updated!.Name);

        var deleteResponse = await client.DeleteAsync($"/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await client.GetAsync($"/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    private sealed record TagRequest(string Name);
    private sealed record TagResponse(Guid Id, string Name);
}
