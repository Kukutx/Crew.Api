using Crew.Api.Data;
using Crew.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EventsDbContext>(options =>
    options.UseInMemoryDatabase("EventsDb"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed in-memory database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
    if (!context.Events.Any())
    {
        context.Events.AddRange(
            new Event
            {
                Id = 1,
                Title = "City Walk",
                Location = "Berlin",
                Description = "A walk through the city center",
                Latitude = 52.520008,
                Longitude = 13.404954,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/city-walk-1.jpg",
                    "https://example.com/images/city-walk-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/city-walk-1.jpg"
            },
            new Event
            {
                Id = 2,
                Title = "Museum Tour",
                Location = "Paris",
                Description = "A tour of the Louvre Museum",
                Latitude = 48.856614,
                Longitude = 2.3522219,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/museum-tour-1.jpg",
                    "https://example.com/images/museum-tour-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/museum-tour-1.jpg"
            },
            new Event
            {
                Id = 3,
                Title = "Coffee Meetup",
                Location = "Paris",
                Description = "聊聊创业和生活",
                Latitude = 48.8566,
                Longitude = 2.3522,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/coffee-meetup-1.jpg",
                    "https://example.com/images/coffee-meetup-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/coffee-meetup-1.jpg"
            },
            new Event
            {
                Id = 4,
                Title = "Art Gallery Walk",
                Location = "Berlin",
                Description = "一起探索当代艺术",
                Latitude = 51.5074,
                Longitude = -0.1278,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/art-gallery-walk-1.jpg",
                    "https://example.com/images/art-gallery-walk-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/art-gallery-walk-1.jpg"
            },
            new Event
            {
                Id = 5,
                Title = "Hiking Adventure",
                Location = "Berlin",
                Description = "阿尔卑斯山徒步",
                Latitude = 46.2044,
                Longitude = 6.1432,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/hiking-adventure-1.jpg",
                    "https://example.com/images/hiking-adventure-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/hiking-adventure-1.jpg"
            },
            new Event
            {
                Id = 6,
                Title = "Board Games Night",
                Location = "Paris",
                Description = "桌游+社交",
                Latitude = 52.5200,
                Longitude = 13.4050,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/board-games-night-1.jpg",
                    "https://example.com/images/board-games-night-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/board-games-night-1.jpg"
            },
            new Event
            {
                Id = 7,
                Title = "Live Music",
                Location = "Berlin",
                Description = "一起听爵士",
                Latitude = 41.9028,
                Longitude = 12.4964,
                ImageUrls = new List<string>
                {
                    "https://example.com/images/live-music-1.jpg",
                    "https://example.com/images/live-music-2.jpg"
                },
                CoverImageUrl = "https://example.com/images/live-music-1.jpg"
            }
        );
        context.SaveChanges();
    }

    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Id = 1, UserName = "alice", Email = "alice@example.com" },
            new User { Id = 2, UserName = "bob", Email = "bob@example.com" }
        );
        context.SaveChanges();
    }

    if (!context.Comments.Any())
    {
        context.Comments.Add(
            new Comment
            {
                Id = 1,
                EventId = 1,
                UserId = 1,
                Content = "Looking forward to it!",
                CreatedAt = DateTime.UtcNow
            }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 启用 Swagger & Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
