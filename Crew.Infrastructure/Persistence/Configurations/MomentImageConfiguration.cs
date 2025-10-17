using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class MomentImageConfiguration : IEntityTypeConfiguration<MomentImage>
{
    public void Configure(EntityTypeBuilder<MomentImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.SortOrder).IsRequired();
    }
}
