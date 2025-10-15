using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class EventSegmentConfiguration : IEntityTypeConfiguration<EventSegment>
{
    public void Configure(EntityTypeBuilder<EventSegment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Seq).IsRequired();
        builder.Property(x => x.Waypoint)
            .IsRequired();
    }
}
