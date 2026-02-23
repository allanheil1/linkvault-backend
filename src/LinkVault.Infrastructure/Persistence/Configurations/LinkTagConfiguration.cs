using LinkVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkVault.Infrastructure.Persistence.Configurations;

public class LinkTagConfiguration : IEntityTypeConfiguration<LinkTag>
{
    public void Configure(EntityTypeBuilder<LinkTag> builder)
    {
        builder.ToTable("link_tags");

        builder.HasKey(lt => new { lt.LinkId, lt.TagId });

        builder.HasOne(lt => lt.Link)
            .WithMany(l => l.LinkTags)
            .HasForeignKey(lt => lt.LinkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lt => lt.Tag)
            .WithMany(t => t.LinkTags)
            .HasForeignKey(lt => lt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
