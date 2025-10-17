using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class UserActivityHistoryConfiguration : IEntityTypeConfiguration<UserActivityHistory>
{
    public void Configure(EntityTypeBuilder<UserActivityHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.OccurredAt).IsRequired();

        builder
            .HasOne(x => x.Event)
            .WithMany(x => x.ActivityHistory)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
