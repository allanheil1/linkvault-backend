using LinkVault.Application.Common.Interfaces;
using LinkVault.Application.Features.Links.Commands;
using LinkVault.Domain.Entities;
using LinkVault.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.UnitTests;

public class LinksCommandHandlersTests
{
    [Fact]
    public async Task CreateLink_ShouldPersistLink_WhenPayloadIsValid()
    {
        await using var context = CreateContext();
        var clock = new FixedDateTimeProvider(new DateTimeOffset(2026, 2, 24, 12, 0, 0, TimeSpan.Zero));
        var userId = await SeedUser(context);
        var handler = new CreateLinkCommandHandler(context, clock);

        var command = new CreateLinkCommand(
            userId,
            "https://example.com",
            "Example",
            "Useful note",
            null,
            Array.Empty<Guid>());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("https://example.com", result.Value!.Url);
        Assert.Equal("Example", result.Value.Title);

        var stored = await context.Links.SingleAsync();
        Assert.Equal(userId, stored.UserId);
        Assert.Equal(clock.UtcNow, stored.CreatedAt);
    }

    [Fact]
    public async Task UpdateLink_ShouldModifyStoredLink_WhenOwnedByUser()
    {
        await using var context = CreateContext();
        var clock = new FixedDateTimeProvider(new DateTimeOffset(2026, 2, 24, 13, 0, 0, TimeSpan.Zero));
        var userId = await SeedUser(context);
        var linkId = await SeedLink(context, userId, "https://old.example", "Old Title");
        var handler = new UpdateLinkCommandHandler(context, clock);

        var command = new UpdateLinkCommand(
            userId,
            linkId,
            "https://new.example",
            "New Title",
            "Updated note",
            null,
            Array.Empty<Guid>(),
            true);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("https://new.example", result.Value!.Url);
        Assert.True(result.Value.IsFavorite);

        var stored = await context.Links.SingleAsync(x => x.Id == linkId);
        Assert.Equal("New Title", stored.Title);
        Assert.Equal(clock.UtcNow, stored.UpdatedAt);
    }

    [Fact]
    public async Task DeleteLink_ShouldRemoveLink_WhenOwnedByUser()
    {
        await using var context = CreateContext();
        var userId = await SeedUser(context);
        var linkId = await SeedLink(context, userId, "https://to-delete.example", "To Delete");
        var handler = new DeleteLinkCommandHandler(context);

        var result = await handler.Handle(new DeleteLinkCommand(userId, linkId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(await context.Links.AnyAsync(x => x.Id == linkId));
    }

    private static LinkVaultDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LinkVaultDbContext>()
            .UseInMemoryDatabase($"linkvault-unit-{Guid.NewGuid()}")
            .Options;

        return new LinkVaultDbContext(options);
    }

    private static async Task<Guid> SeedUser(LinkVaultDbContext context)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = $"test-{Guid.NewGuid():N}@local",
            PasswordHash = "hash",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);
        return user.Id;
    }

    private static async Task<Guid> SeedLink(LinkVaultDbContext context, Guid userId, string url, string title)
    {
        var link = new Link
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Url = url,
            Title = title,
            IsFavorite = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Links.Add(link);
        await context.SaveChangesAsync(CancellationToken.None);
        return link.Id;
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public FixedDateTimeProvider(DateTimeOffset now)
        {
            UtcNow = now;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
