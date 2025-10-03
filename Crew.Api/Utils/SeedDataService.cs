using System;
using System.Collections.Generic;
using System.Linq;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;

namespace Crew.Api.Utils;

public static class SeedDataService
{
    public static void SeedDatabase(AppDbContext context)
    {
        var userUids = new[]
        {
            "znJ7TKLY6CfJA7erPijkGGmHUMo2",
            "DcklGiovAyY6KK5kRJb1saTE0ue2",
            "kujZMzf2m4Wkz0iuJ1Xofpe0yb83",
            "msO0VorfTgdZm9tBfesm60fdbzm1",
            "Xm26B0NPATNtUhK2YJZjsHFdHXD2",
            "0cO4ZIGOtWTfISF5XcHEV2JIfMl1",
            "noOmQhhX1fc5EuCrMEO7n8VScrS2",
            "0kl6ETYUu2Ugclow94CBgSUoIEo2"
        };

        if (!context.Users.Any())
        {
            var seededUsers = new List<UserAccount>
            {
                CreateUser(userUids[0], "admin@casl.io", "admin", "Admin User"),
                CreateUser(userUids[1], "alice@example.com", "alice", "Alice"),
                CreateUser(userUids[2], "bob@example.com", "bob", "Bob"),
                CreateUser(userUids[3], "charlie@example.com", "charlie", "Charlie"),
                CreateUser(userUids[4], "diana@example.com", "diana", "Diana"),
                CreateUser(userUids[5], "eric@example.com", "eric", "Eric"),
                CreateUser(userUids[6], "fiona@example.com", "fiona", "Fiona"),
                CreateUser(userUids[7], "george@example.com", "george", "George")
            };

            context.Users.AddRange(seededUsers);
            context.SaveChanges();
        }

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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[0]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[1]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[2]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[3]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[4]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[5]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[6]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[7]),
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
                    coverImageUrl: "https://i.imgur.com/c7BHAnI.png",
                    userUid: userUids[0])
            };

            context.Events.AddRange(seededEvents);
            context.SaveChanges();
        }

        if (!context.Roles.Any())
        {
            var roles = new List<Role>
            {
                new Role { Id = 1, Key = RoleKeys.User, DisplayName = "User", IsSystemRole = true },
                new Role { Id = 2, Key = RoleKeys.Admin, DisplayName = "Admin", IsSystemRole = true },
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        if (!context.SubscriptionPlans.Any())
        {
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Id = 1,
                    Key = "free",
                    DisplayName = "Free",
                    Description = "基础免费计划",
                    SortOrder = 0
                },
                new SubscriptionPlan
                {
                    Id = 2,
                    Key = "tier1",
                    DisplayName = "Tier 1",
                    Description = "基础付费计划",
                    SortOrder = 1
                },
                new SubscriptionPlan
                {
                    Id = 3,
                    Key = "tier2",
                    DisplayName = "Tier 2",
                    Description = "进阶计划，更多额度",
                    SortOrder = 2
                },
                new SubscriptionPlan
                {
                    Id = 4,
                    Key = "tier3",
                    DisplayName = "Tier 3",
                    Description = "高级计划，解锁全部功能",
                    SortOrder = 3
                }
            };

            context.SubscriptionPlans.AddRange(plans);
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
        string coverImageUrl,
        string userUid)
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
            CoverImageUrl = cover,
            UserUid = userUid
        };
    }

    private static UserAccount CreateUser(string uid, string email, string userName, string displayName)
    {
        return new UserAccount
        {
            Uid = uid,
            Email = email,
            UserName = userName,
            DisplayName = displayName,
            Bio = string.Empty,
            AvatarUrl = AvatarDefaults.FallbackUrl,
            CoverImageUrl = string.Empty,
            Status = UserStatuses.Active,
            CreatedAt = DateTime.UtcNow
        };
    }
}
