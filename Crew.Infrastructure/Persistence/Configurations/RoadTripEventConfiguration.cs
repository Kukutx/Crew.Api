using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class RoadTripEventConfiguration : IEntityTypeConfiguration<RoadTripEvent>
{
    public void Configure(EntityTypeBuilder<RoadTripEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
        builder.Property(x => x.StartPoint)
            .HasColumnType("geography(Point,4326)")
            .IsRequired();
        builder.Property(x => x.EndPoint)
            .HasColumnType("geography(Point,4326)");
        builder.HasIndex(x => x.StartPoint).HasMethod("GIST");
        builder.HasIndex(x => x.EndPoint).HasMethod("GIST");
        builder.Property(x => x.RoutePolyline).HasMaxLength(4096);
        builder.HasMany(x => x.Segments)
            .WithOne(x => x.Event!)
            .HasForeignKey(x => x.EventId);
        builder.HasMany(x => x.Registrations)
            .WithOne(x => x.Event!)
            .HasForeignKey(x => x.EventId);
    }
}
