using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crew.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Kind).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Seq).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.BodyText).HasColumnType("text");
        builder.Property(x => x.MetaJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.ChatId, x.Seq })
            .IsUnique()
            .IsDescending(false, true)
            .HasDatabaseName("IX_ChatMessage_ChatId_Seq");
        builder.HasIndex(x => x.ChatId).HasDatabaseName("IX_ChatMessage_ChatId");
    }
}
