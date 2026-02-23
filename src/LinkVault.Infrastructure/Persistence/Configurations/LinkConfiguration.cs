using LinkVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkVault.Infrastructure.Persistence.Configurations;

public class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("links");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Note)
            .HasMaxLength(2000);

        builder.Property(l => l.IsFavorite)
            .HasDefaultValue(false);

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(l => l.User)
            .WithMany(u => u.Links)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Collection)
            .WithMany(c => c!.Links)
            .HasForeignKey(l => l.CollectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
