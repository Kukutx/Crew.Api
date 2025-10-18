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
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now() at time zone 'utc'");
        builder.Property(x => x.StartPoint)
            .HasColumnType("geometry (Point, 4326)")
            .IsRequired();
        builder.Property(x => x.EndPoint)
            .HasColumnType("geometry (Point, 4326)");
        builder.Property(x => x.Location)
            .HasColumnType("geometry (Point, 4326)")
            .IsRequired();
        builder.Property(x => x.RoutePolyline).HasMaxLength(4096);
        builder.Property(x => x.MaxParticipants).HasDefaultValue(7);

        builder.HasMany(x => x.Segments)
            .WithOne(x => x.Event!)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Registrations)
            .WithOne(x => x.Event!)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Moments)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.ActivityHistory)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Metrics)
            .WithOne(x => x.Event)
            .HasForeignKey<EventMetrics>(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
