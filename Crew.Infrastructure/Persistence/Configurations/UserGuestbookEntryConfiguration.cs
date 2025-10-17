using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class UserGuestbookEntryConfiguration : IEntityTypeConfiguration<UserGuestbookEntry>
{
    public void Configure(EntityTypeBuilder<UserGuestbookEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.Rating);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne(x => x.Author)
            .WithMany(x => x.AuthoredGuestbookEntries)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Owner)
            .WithMany(x => x.GuestbookEntries)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
