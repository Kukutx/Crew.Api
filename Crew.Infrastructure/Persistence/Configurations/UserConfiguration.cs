using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirebaseUid).IsRequired().HasMaxLength(128);
        builder.HasIndex(x => x.FirebaseUid).IsUnique();
        builder.Property(x => x.DisplayName).HasMaxLength(256);
    }
}
