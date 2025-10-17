using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class EventTagConfiguration : IEntityTypeConfiguration<EventTag>
{
    public void Configure(EntityTypeBuilder<EventTag> builder)
    {
        builder.HasKey(x => new { x.EventId, x.TagId });
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
