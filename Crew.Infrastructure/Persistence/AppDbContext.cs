using Crew.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crew.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RoadTripEvent> RoadTripEvents => Set<RoadTripEvent>();
    public DbSet<EventSegment> EventSegments => Set<EventSegment>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatMessageAttachment> ChatMessageAttachments => Set<ChatMessageAttachment>();
    public DbSet<ChatMessageReaction> ChatMessageReactions => Set<ChatMessageReaction>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UserTag> UserTags => Set<UserTag>();
    public DbSet<EventTag> EventTags => Set<EventTag>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<UserActivityHistory> UserActivityHistories => Set<UserActivityHistory>();
    public DbSet<UserGuestbookEntry> UserGuestbookEntries => Set<UserGuestbookEntry>();
    public DbSet<Moment> Moments => Set<Moment>();
    public DbSet<MomentImage> MomentImages => Set<MomentImage>();
    public DbSet<MomentComment> MomentComments => Set<MomentComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
