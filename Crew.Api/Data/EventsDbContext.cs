using Crew.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Crew.Api.Data;

public class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
}

