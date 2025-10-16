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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
