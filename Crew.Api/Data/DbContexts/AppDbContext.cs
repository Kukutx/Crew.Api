using Crew.Api.Data.Converters;
using Crew.Api.Entities;
using Crew.Api.Extensions;
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
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripRoute> TripRoutes => Set<TripRoute>();
    public DbSet<TripSchedule> TripSchedules => Set<TripSchedule>();
    public DbSet<TripParticipant> TripParticipants => Set<TripParticipant>();
    public DbSet<TripComment> TripComments => Set<TripComment>();
    public DbSet<TripFavorite> TripFavorites => Set<TripFavorite>();
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
            entity.Property(u => u.Status)
                .HasConversion(new EnumMemberValueConverter<UserStatus>())
                .HasMaxLength(32)
                .HasDefaultValue(UserStatus.Active.GetEnumMemberValue());
            entity.Property(u => u.IdentityLabel)
                .HasConversion(new EnumMemberValueConverter<UserIdentityLabel>())
                .HasMaxLength(32)
                .HasDefaultValue(UserIdentityLabel.Visitor.GetEnumMemberValue());
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
            entity.ToTable("Events");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.ToTable("Trips");

            entity.Property(t => t.StartLocation)
                .HasMaxLength(256);
            entity.Property(t => t.EndLocation)
                .HasMaxLength(256);
            entity.Property(t => t.ItineraryDescription)
                .HasMaxLength(2048);

            entity.HasMany(t => t.Routes)
                .WithOne(r => r.Trip!)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Schedules)
                .WithOne(s => s.Trip!)
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Participants)
                .WithOne(p => p.Trip!)
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.TripComments)
                .WithOne(c => c.Trip!)
                .HasForeignKey(c => c.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.TripFavorites)
                .WithOne(f => f.Trip!)
                .HasForeignKey(f => f.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.TripImages)
                .WithOne(i => i.Trip!)
                .HasForeignKey(i => i.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripRoute>(entity =>
        {
            entity.ToTable("TripRoutes");
            entity.HasIndex(r => new { r.TripId, r.OrderIndex }).IsUnique();
            entity.Property(r => r.Name)
                .HasMaxLength(256);
            entity.Property(r => r.Description)
                .HasMaxLength(512);
        });

        modelBuilder.Entity<TripSchedule>(entity =>
        {
            entity.ToTable("TripSchedules");
            entity.Property(ts => ts.Content)
                .HasMaxLength(1024);
            entity.Property(ts => ts.Hotel)
                .HasMaxLength(256);
            entity.Property(ts => ts.Meal)
                .HasMaxLength(256);
            entity.Property(ts => ts.Note)
                .HasMaxLength(512);
        });

        modelBuilder.Entity<TripParticipant>(entity =>
        {
            entity.ToTable("TripParticipants");
            entity.HasIndex(tp => new { tp.TripId, tp.UserUid }).IsUnique();

            entity.Property(tp => tp.Role)
                .HasConversion(new EnumMemberValueConverter<TripParticipantRole>())
                .HasMaxLength(32)
                .HasDefaultValue(TripParticipantRole.Passenger.GetEnumMemberValue());
            entity.Property(tp => tp.Status)
                .HasConversion(new EnumMemberValueConverter<TripParticipantStatus>())
                .HasMaxLength(32)
                .HasDefaultValue(TripParticipantStatus.Pending.GetEnumMemberValue());
            entity.Property(tp => tp.JoinTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(tp => tp.User)
                .WithMany(u => u.TripParticipations)
                .HasForeignKey(tp => tp.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripComment>(entity =>
        {
            entity.ToTable("TripComments");
            entity.Property(tc => tc.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(tc => tc.Rating)
                .HasDefaultValue(0);
            entity.Property(tc => tc.Content)
                .HasMaxLength(1024);

            entity.HasOne(tc => tc.User)
                .WithMany(u => u.TripComments)
                .HasForeignKey(tc => tc.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripFavorite>(entity =>
        {
            entity.ToTable("TripFavorites");
            entity.HasKey(tf => new { tf.TripId, tf.UserUid });
            entity.Property(tf => tf.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(tf => tf.User)
                .WithMany(u => u.TripFavorites)
                .HasForeignKey(tf => tf.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripImage>(entity =>
        {
            entity.ToTable("TripImages");
            entity.Property(ti => ti.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(ti => ti.Url)
                .HasMaxLength(512);

            entity.HasOne(ti => ti.Uploader)
                .WithMany(u => u.TripImages)
                .HasForeignKey(ti => ti.UploaderUid)
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
                .HasConversion(new EnumMemberValueConverter<EventRegistrationStatus>())
                .HasMaxLength(32)
                .HasDefaultValue(EventRegistrationStatus.Pending.GetEnumMemberValue());
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
