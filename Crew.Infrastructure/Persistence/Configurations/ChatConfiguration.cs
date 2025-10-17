using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(256);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasMany(x => x.Members)
            .WithOne(x => x.Chat!)
            .HasForeignKey(x => x.ChatId);
        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Chat!)
            .HasForeignKey(x => x.ChatId);
        builder.HasIndex(x => new { x.EventId }).HasDatabaseName("IX_Chat_EventId");
        builder.HasIndex(x => new { x.Type, x.OwnerUserId });
    }
}
