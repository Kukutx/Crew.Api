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
    public DbSet<TestData> TestData { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

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

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

