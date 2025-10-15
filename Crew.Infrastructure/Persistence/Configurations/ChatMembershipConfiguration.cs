using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class ChatMembershipConfiguration : IEntityTypeConfiguration<ChatMembership>
{
    public void Configure(EntityTypeBuilder<ChatMembership> builder)
    {
        builder.HasKey(x => new { x.GroupId, x.UserId });
        builder.Property(x => x.Role).IsRequired().HasMaxLength(32);
        builder.Property(x => x.JoinedAt).IsRequired();
    }
}
