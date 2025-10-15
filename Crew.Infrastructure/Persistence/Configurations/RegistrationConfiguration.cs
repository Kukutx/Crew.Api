using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
