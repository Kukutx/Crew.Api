using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class EventMetricsConfiguration : IEntityTypeConfiguration<EventMetrics>
{
    public void Configure(EntityTypeBuilder<EventMetrics> builder)
    {
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.LikesCount).HasDefaultValue(0);
        builder.Property(x => x.RegistrationsCount).HasDefaultValue(0);
        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now() at time zone 'utc'");
    }
}
