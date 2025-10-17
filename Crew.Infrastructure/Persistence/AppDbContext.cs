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
    public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
    public DbSet<ChatMembership> ChatMemberships => Set<ChatMembership>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<PrivateDialog> PrivateDialogs => Set<PrivateDialog>();
    public DbSet<PrivateMessage> PrivateMessages => Set<PrivateMessage>();
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
