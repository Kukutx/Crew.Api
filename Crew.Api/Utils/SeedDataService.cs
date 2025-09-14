using Crew.Api.Data;
using Crew.Api.Models;

namespace Crew.Api.Utils
{
    public static class SeedDataService
    {
        public static void SeedDatabase(EventsDbContext context)
        {
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
                                        "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
"https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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
                                    "https://images.unsplash.com/photo-1520975928316-56c6f6f163a4",
"https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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
                                    "https://images.unsplash.com/photo-1519681393784-d120267933ba",
"https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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
                                    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
"https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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
                                    "https://images.unsplash.com/photo-1500534314209-a25ddb2bd429",
"https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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
                                    "https://images.unsplash.com/photo-1472214103451-9374bd1c798e",
"https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
    "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66",
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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
                                },
                                CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
                            },
                                               new Event
                                               {
                                                   Id = 8,
                                                   Title = "Live Music",
                                                   Location = "Milan",
                                                   Description = "一起听音乐",
                                                   Latitude = 31.9028,
                                                   Longitude = 12.4964,
                                                   ImageUrls = new List<string>
                                                   {
                                                       "https://images.unsplash.com/photo-1469474968028-56623f02e42e",
                                                   },
                                                   CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
                                               },
                                                                    new Event
                                                                    {
                                                                        Id = 9,
                                                                        Title = "Sport",
                                                                        Location = "Roma",
                                                                        Description = "运动",
                                                                        Latitude = 21.9028,
                                                                        Longitude = 22.4964,
                                                                        ImageUrls = new List<string>
                                                   {
                                                       "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?ixlib=rb-1.2.1",
                                                   },
                                                                        CoverImageUrl = "https://i.imgur.com/c7BHAnI.png"
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

    }
}