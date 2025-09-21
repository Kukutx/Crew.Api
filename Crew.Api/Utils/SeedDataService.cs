using System;
using System.Collections.Generic;
using System.Linq;
using Crew.Api.Data;
using Crew.Api.Models;

namespace Crew.Api.Utils;

public static class SeedDataService
{
    public static void SeedDatabase(EventsDbContext context)
    {
        if (!context.Events.Any())
        {
            var baseTime = DateTime.UtcNow.Date.AddHours(10);

            var seededEvents = new List<Event>
            {
                CreateEvent(
                    id: 1,
                    title: "City Walk",
                    type: "Outdoor",
                    status: "Upcoming",
                    organizer: "Urban Explorers",
                    location: "Berlin",
                    description: "A walk through the city center",
                    expectedParticipants: 40,
                    startTime: baseTime.AddDays(1),
                    endTime: baseTime.AddDays(1).AddHours(2),
                    latitude: 52.520008,
                    longitude: 13.404954,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                        "https://images.unsplash.com/photo-1529927066849-a6c9a73b73a0",
                        "https://images.unsplash.com/photo-1508057198894-247b23fe5ade"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 2,
                    title: "Museum Tour",
                    type: "Culture",
                    status: "Upcoming",
                    organizer: "Art Lovers Club",
                    location: "Paris",
                    description: "A tour of the Louvre Museum",
                    expectedParticipants: 30,
                    startTime: baseTime.AddDays(2),
                    endTime: baseTime.AddDays(2).AddHours(4),
                    latitude: 48.856614,
                    longitude: 2.3522219,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1520975928316-56c6f6f163a4",
                        "https://images.unsplash.com/photo-1529429617124-aee30bd7e8f9",
                        "https://images.unsplash.com/photo-1441974231531-c6227db76b6e"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 3,
                    title: "Coffee Meetup",
                    type: "Networking",
                    status: "Upcoming",
                    organizer: "Startup Stories",
                    location: "Paris",
                    description: "聊聊创业和生活",
                    expectedParticipants: 20,
                    startTime: baseTime.AddDays(3).AddHours(2),
                    endTime: baseTime.AddDays(3).AddHours(4),
                    latitude: 48.8566,
                    longitude: 2.3522,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1519681393784-d120267933ba",
                        "https://images.unsplash.com/photo-1466978913421-dad2ebd01d17",
                        "https://images.unsplash.com/photo-1470337458703-46ad1756a187"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 4,
                    title: "Art Gallery Walk",
                    type: "Culture",
                    status: "Upcoming",
                    organizer: "Creative Fridays",
                    location: "Berlin",
                    description: "一起探索当代艺术",
                    expectedParticipants: 25,
                    startTime: baseTime.AddDays(4).AddHours(1),
                    endTime: baseTime.AddDays(4).AddHours(3),
                    latitude: 52.5200,
                    longitude: 13.4050,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1529101091764-c3526daf38fe",
                        "https://images.unsplash.com/photo-1487412912498-0447578fcca8",
                        "https://images.unsplash.com/photo-1496317899792-9d7dbcd928a1"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 5,
                    title: "Hiking Adventure",
                    type: "Outdoor",
                    status: "Planning",
                    organizer: "Alpine Treks",
                    location: "Zurich",
                    description: "阿尔卑斯山徒步",
                    expectedParticipants: 18,
                    startTime: baseTime.AddDays(7),
                    endTime: baseTime.AddDays(7).AddHours(6),
                    latitude: 46.2044,
                    longitude: 6.1432,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1500534314209-a25ddb2bd429",
                        "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                        "https://images.unsplash.com/photo-1469474968028-56623f02e42e"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 6,
                    title: "Board Games Night",
                    type: "Community",
                    status: "Completed",
                    organizer: "Tabletop Circle",
                    location: "Paris",
                    description: "桌游+社交",
                    expectedParticipants: 16,
                    startTime: baseTime.AddDays(-2).AddHours(18),
                    endTime: baseTime.AddDays(-2).AddHours(22),
                    latitude: 48.8566,
                    longitude: 2.3522,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1472214103451-9374bd1c798e",
                        "https://images.unsplash.com/photo-1489515217757-5fd1be406fef",
                        "https://images.unsplash.com/photo-1521737604893-d14cc237f11d"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 7,
                    title: "Live Jazz Night",
                    type: "Music",
                    status: "Upcoming",
                    organizer: "Blue Note Crew",
                    location: "Berlin",
                    description: "一起听爵士",
                    expectedParticipants: 60,
                    startTime: baseTime.AddDays(5).AddHours(20),
                    endTime: baseTime.AddDays(5).AddHours(23),
                    latitude: 52.520008,
                    longitude: 13.404954,
                    imageUrls: Array.Empty<string>(),
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 8,
                    title: "Open Air Concert",
                    type: "Music",
                    status: "Upcoming",
                    organizer: "Milan Sound Collective",
                    location: "Milan",
                    description: "一起听音乐",
                    expectedParticipants: 120,
                    startTime: baseTime.AddDays(6).AddHours(19),
                    endTime: baseTime.AddDays(6).AddHours(23),
                    latitude: 45.4642,
                    longitude: 9.19,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1469474968028-56623f02e42e",
                        "https://images.unsplash.com/photo-1497032628192-86f99bcd76bc",
                        "https://images.unsplash.com/photo-1506157786151-b8491531f063"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png"),
                CreateEvent(
                    id: 9,
                    title: "Morning Run Club",
                    type: "Sports",
                    status: "Upcoming",
                    organizer: "Roma Runners",
                    location: "Rome",
                    description: "运动",
                    expectedParticipants: 35,
                    startTime: baseTime.AddDays(8).AddHours(7),
                    endTime: baseTime.AddDays(8).AddHours(9),
                    latitude: 41.9028,
                    longitude: 12.4964,
                    imageUrls: new[]
                    {
                        "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                        "https://images.unsplash.com/photo-1482192596544-9eb780fc7f66"
                    },
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png")
            };

            context.Events.AddRange(seededEvents);
            context.SaveChanges();
        }

        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Id = 1,
                    UserName = "alice",
                    Email = "alice@example.com",
                    Uid = "user-alice",
                    Name = "Alice Lee",
                    Bio = "Berlin-based community organizer who loves urban walks.",
                    Avatar = "https://i.imgur.com/zY5R8dH.png",
                    Cover = "https://i.imgur.com/3S9g6Et.png",
                    Followers = 128,
                    Following = 54,
                    Likes = 640,
                    Followed = true
                },
                new User
                {
                    Id = 2,
                    UserName = "bob",
                    Email = "bob@example.com",
                    Uid = "user-bob",
                    Name = "Bob Martin",
                    Bio = "A Paris-based designer and museum enthusiast.",
                    Avatar = "https://i.imgur.com/4ZQZ3p0.png",
                    Cover = "https://i.imgur.com/2bE8wE7.png",
                    Followers = 96,
                    Following = 73,
                    Likes = 480,
                    Followed = false
                },
                new User
                {
                    Id = 3,
                    UserName = "carol",
                    Email = "carol@example.com",
                    Uid = "user-carol",
                    Name = "Carol Smith",
                    Bio = "Event host who curates intimate coffee meetups.",
                    Avatar = "https://i.imgur.com/V0YqR0P.png",
                    Cover = "https://i.imgur.com/8aZPRWn.png",
                    Followers = 205,
                    Following = 120,
                    Likes = 1024,
                    Followed = false
                }
            );
            context.SaveChanges();
        }

        if (!context.Comments.Any())
        {
            context.Comments.AddRange(
                new Comment
                {
                    Id = 1,
                    EventId = 1,
                    UserId = 1,
                    Content = "Looking forward to it!",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Comment
                {
                    Id = 2,
                    EventId = 2,
                    UserId = 2,
                    Content = "Count me in for the museum tour.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Comment
                {
                    Id = 3,
                    EventId = 3,
                    UserId = 3,
                    Content = "Will there be coffee tastings?",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                }
            );
            context.SaveChanges();
        }
    }

    private static Event CreateEvent(
        int id,
        string title,
        string type,
        string status,
        string organizer,
        string location,
        string description,
        int expectedParticipants,
        DateTime startTime,
        DateTime endTime,
        double latitude,
        double longitude,
        IEnumerable<string> imageUrls,
        string coverImageUrl)
    {
        var images = imageUrls?
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.Trim())
            .Take(5)
            .ToList() ?? new List<string>();

        var cover = !string.IsNullOrWhiteSpace(coverImageUrl)
            ? coverImageUrl.Trim()
            : images.FirstOrDefault() ?? string.Empty;

        var createdAt = startTime.AddDays(-7);
        var lastUpdated = createdAt.AddDays(2);
        if (lastUpdated < createdAt)
        {
            lastUpdated = createdAt;
        }

        if (endTime < startTime)
        {
            endTime = startTime;
        }

        return new Event
        {
            Id = id,
            Title = title,
            Type = type,
            Status = status,
            Organizer = organizer,
            Location = location,
            Description = description,
            ExpectedParticipants = Math.Max(0, expectedParticipants),
            StartTime = startTime,
            EndTime = endTime,
            CreatedAt = createdAt,
            LastUpdated = lastUpdated,
            Latitude = latitude,
            Longitude = longitude,
            ImageUrls = images,
            CoverImageUrl = cover
        };
    }
}
