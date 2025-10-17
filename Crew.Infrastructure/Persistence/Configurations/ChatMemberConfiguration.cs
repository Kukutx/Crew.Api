using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class ChatMemberConfiguration : IEntityTypeConfiguration<ChatMember>
{
    public void Configure(EntityTypeBuilder<ChatMember> builder)
    {
        builder.HasKey(x => new { x.ChatId, x.UserId });
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();
        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_ChatMember_UserId");
        builder.HasIndex(x => x.ChatId).HasDatabaseName("IX_ChatMember_ChatId");
    }
}
