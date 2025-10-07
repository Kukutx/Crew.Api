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
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<EventFavorite> EventFavorites => Set<EventFavorite>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

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

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserUid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Event)
                .WithMany(e => e.Comments)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventFavorite>(entity =>
        {
            entity.ToTable("EventFavorites");
            entity.HasKey(f => new { f.EventId, f.UserUid });
            entity.Property(f => f.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(f => f.Event)
                .WithMany(e => e.Favorites)
                .HasForeignKey(f => f.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.User)
                .WithMany(u => u.FavoriteEvents)
                .HasForeignKey(f => f.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.ToTable("EventRegistrations");
            entity.HasKey(r => new { r.EventId, r.UserUid });
            entity.Property(r => r.RegisteredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(r => r.Status)
                .HasMaxLength(32)
                .HasDefaultValue(EventRegistrationStatuses.Pending);
            entity.Property(r => r.StatusUpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany(u => u.EventRegistrations)
                .HasForeignKey(r => r.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
