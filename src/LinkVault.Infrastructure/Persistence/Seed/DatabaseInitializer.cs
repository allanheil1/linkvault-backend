using BCrypt.Net;
using LinkVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkVault.Infrastructure.Persistence.Seed;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly LinkVaultDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(LinkVaultDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);
        await SeedDemoDataAsync(cancellationToken);
    }

    private async Task SeedDemoDataAsync(CancellationToken cancellationToken)
    {
        const string demoEmail = "demo@linkvault.local";
        const string demoPassword = "Demo123!";

        var now = DateTimeOffset.UtcNow;

        var demoUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == demoEmail, cancellationToken);

        if (demoUser == null)
        {
            demoUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo User",
                Email = demoEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(demoPassword),
                CreatedAt = now
            };

            _context.Users.Add(demoUser);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded demo user");
        }

        // Collections
        var collectionNames = new[] { "Personal", "Work" };
        var collections = new Dictionary<string, Collection>();

        foreach (var name in collectionNames)
        {
            var collection = await _context.Collections
                .FirstOrDefaultAsync(c => c.UserId == demoUser.Id && c.Name == name, cancellationToken);

            if (collection == null)
            {
                collection = new Collection
                {
                    Id = Guid.NewGuid(),
                    UserId = demoUser.Id,
                    Name = name,
                    CreatedAt = now
                };
                _context.Collections.Add(collection);
            }

            collections[name] = collection;
        }

        // Tags
        var tagNames = new[] { "productivity", "reading", "inspiration" };
        var tags = new Dictionary<string, Tag>();

        foreach (var name in tagNames)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.UserId == demoUser.Id && t.Name == name, cancellationToken);

            if (tag == null)
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    UserId = demoUser.Id,
                    Name = name
                };
                _context.Tags.Add(tag);
            }

            tags[name] = tag;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var linkBlueprints = new[]
        {
            new
            {
                Url = "https://martinfowler.com/articles/microservices.html",
                Title = "Microservices by Martin Fowler",
                Note = "Great deep dive on microservices trade-offs.",
                Collection = (string?)"Work",
                TagNames = new[] { "productivity" },
                IsFavorite = true
            },
            new
            {
                Url = "https://news.ycombinator.com/",
                Title = "Hacker News",
                Note = "Daily tech news feed.",
                Collection = (string?)"Personal",
                TagNames = new[] { "reading", "inspiration" },
                IsFavorite = false
            },
            new
            {
                Url = "https://www.youtube.com/watch?v=K8YELRmUb5o",
                Title = "The Mind Behind Linux (TED)",
                Note = "Good for inspiration.",
                Collection = (string?)null,
                TagNames = new[] { "inspiration" },
                IsFavorite = false
            }
        };

        foreach (var blueprint in linkBlueprints)
        {
            var existingLink = await _context.Links
                .Include(l => l.LinkTags)
                .FirstOrDefaultAsync(l => l.UserId == demoUser.Id && l.Url == blueprint.Url, cancellationToken);

            if (existingLink != null)
            {
                continue;
            }

            var link = new Link
            {
                Id = Guid.NewGuid(),
                UserId = demoUser.Id,
                CollectionId = blueprint.Collection != null ? collections[blueprint.Collection].Id : null,
                Url = blueprint.Url,
                Title = blueprint.Title,
                Note = blueprint.Note,
                IsFavorite = blueprint.IsFavorite,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var tagName in blueprint.TagNames)
            {
                var tag = tags[tagName];
                link.LinkTags.Add(new LinkTag
                {
                    LinkId = link.Id,
                    TagId = tag.Id,
                    Tag = tag
                });
            }

            _context.Links.Add(link);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
