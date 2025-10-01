using Crew.Api.Models;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Data.DbContexts;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DomainUsers> DomainUsers => Set<DomainUsers>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TestData> TestData { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
}

