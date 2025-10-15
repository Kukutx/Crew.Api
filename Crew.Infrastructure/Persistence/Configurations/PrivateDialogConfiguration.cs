using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class PrivateDialogConfiguration : IEntityTypeConfiguration<PrivateDialog>
{
    public void Configure(EntityTypeBuilder<PrivateDialog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserA, x.UserB }).IsUnique();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
