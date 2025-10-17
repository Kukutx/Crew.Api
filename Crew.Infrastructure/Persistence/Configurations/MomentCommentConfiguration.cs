using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public sealed class MomentCommentConfiguration : IEntityTypeConfiguration<MomentComment>
{
    public void Configure(EntityTypeBuilder<MomentComment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
