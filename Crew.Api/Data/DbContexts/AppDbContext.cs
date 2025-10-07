using Crew.Api.Entities;
using Crew.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Data.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRoleAssignment> UserRoles => Set<UserRoleAssignment>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripComment> TripComments => Set<TripComment>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<TripFavorite> TripFavorites => Set<TripFavorite>();
    public DbSet<TripParticipant> TripParticipants => Set<TripParticipant>();
    public DbSet<TripRoute> TripRoutes => Set<TripRoute>();
    public DbSet<TripSchedule> TripSchedules => Set<TripSchedule>();
    public DbSet<TripImage> TripImages => Set<TripImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Uid);
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasMany(u => u.Roles)
                .WithOne(r => r.User!)
                .HasForeignKey(r => r.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(u => u.Subscriptions)
                .WithOne(s => s.User!)
                .HasForeignKey(s => s.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(u => u.Followers)
                .WithOne(f => f.Followed)
                .HasForeignKey(f => f.FollowedUid)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(u => u.Following)
                .WithOne(f => f.Follower)
                .HasForeignKey(f => f.FollowerUid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(u => u.OrganizedEvents)
                .WithOne(e => e.Organizer!)
                .HasForeignKey(e => e.OrganizerUid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(u => u.IdentityLabel)
                .HasMaxLength(32)
                .HasDefaultValue(UserIdentityLabels.Visitor);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(r => r.Key).IsUnique();
        });

        modelBuilder.Entity<UserRoleAssignment>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ura => new { ura.UserUid, ura.RoleId });
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("Plans");
            entity.HasIndex(p => p.Key).IsUnique();
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("UserSubscriptions");
            entity.HasKey(us => new { us.UserUid, us.PlanId });
        });

        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.ToTable("UserFollows");
            entity.HasKey(uf => new { uf.FollowerUid, uf.FollowedUid });
            entity.Property(uf => uf.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(uf => uf.FollowedUid);
            entity.HasIndex(uf => uf.FollowerUid);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.Property(e => e.Status)
                .HasMaxLength(32)
                .HasDefaultValue(EventStatuses.Draft);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.OrganizerUid);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.ToTable("Trips");
        });

        modelBuilder.Entity<TripRoute>(entity =>
        {
            entity.ToTable("TripRoutes");
            entity.HasIndex(r => new { r.TripId, r.OrderIndex })
                .IsUnique();

            entity.HasOne(r => r.Trip)
                .WithMany(t => t.Routes)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripSchedule>(entity =>
        {
            entity.ToTable("TripSchedules");

            entity.HasOne(s => s.Trip)
                .WithMany(t => t.Schedules)
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripImage>(entity =>
        {
            entity.ToTable("TripImages");

            entity.HasOne(i => i.Trip)
                .WithMany(t => t.Images)
                .HasForeignKey(i => i.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripComment>(entity =>
        {
            entity.ToTable("TripComments");
            entity.Property(c => c.Rating)
                .HasDefaultValue(0);

            entity.HasOne(c => c.Trip)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripFavorite>(entity =>
        {
            entity.ToTable("TripFavorites");
            entity.HasKey(f => new { f.TripId, f.UserUid });
            entity.Property(f => f.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(f => f.Trip)
                .WithMany(t => t.Favorites)
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.User)
                .WithMany(u => u.FavoriteTrips)
                .HasForeignKey(f => f.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripParticipant>(entity =>
        {
            entity.ToTable("TripParticipants");
            entity.HasKey(p => new { p.TripId, p.UserUid });
            entity.Property(p => p.JoinTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(p => p.Status)
                .HasMaxLength(32)
                .HasDefaultValue(TripParticipantStatuses.Pending);

            entity.HasOne(p => p.Trip)
                .WithMany(t => t.Participants)
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.User)
                .WithMany(u => u.TripParticipants)
                .HasForeignKey(p => p.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
