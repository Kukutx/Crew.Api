using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class MomentConfiguration : IEntityTypeConfiguration<Moment>
{
    public void Configure(EntityTypeBuilder<Moment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Content).HasMaxLength(2048);
        builder.Property(x => x.Country).IsRequired().HasMaxLength(128);
        builder.Property(x => x.City).HasMaxLength(128);
        builder.Property(x => x.CoverImageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne(x => x.Event)
            .WithMany(x => x.Moments)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
