using System;
using System.Collections.Generic;
using System.Linq;
using Crew.Api.Data.DbContexts;
using Crew.Api.Entities;
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

        if (!context.Trips.Any())
        {
            var baseDate = DateTime.UtcNow.Date.AddDays(3);

            var seededTrips = new List<Trip>
            {
                CreateTrip(
                    title: "川西环线自驾",
                    organizerUid: userUids[1],
                    startDate: baseDate,
                    endDate: baseDate.AddDays(5),
                    startLocation: "成都",
                    endLocation: "成都",
                    description: "5 天环线探索川西秘境，适合 4-6 辆车同行。",
                    status: TripStatuses.Published,
                    expectedParticipants: 12,
                    coverImageUrl: "https://images.unsplash.com/photo-1499696010181-ef48a3d9d4c4",
                    startLatitude: 30.5728,
                    startLongitude: 104.0668,
                    endLatitude: 30.5728,
                    endLongitude: 104.0668,
                    routes: new[]
                    {
                        new TripRoute { OrderIndex = 1, Name = "成都集合", Latitude = 30.5728, Longitude = 104.0668, Description = "车友会面及车辆检查" },
                        new TripRoute { OrderIndex = 2, Name = "四姑娘山", Latitude = 31.0594, Longitude = 102.8790, Description = "观景拍照" },
                        new TripRoute { OrderIndex = 3, Name = "丹巴藏寨", Latitude = 30.8783, Longitude = 101.8861, Description = "入住民宿体验藏式文化" },
                        new TripRoute { OrderIndex = 4, Name = "成都返程", Latitude = 30.5728, Longitude = 104.0668, Description = "总结分享" },
                    },
                    schedules: new[]
                    {
                        new TripSchedule { Date = baseDate, Content = "成都集合，车辆检查", Hotel = "成都首日晚酒店", Meal = "欢迎晚餐", Note = "晚上自由活动" },
                        new TripSchedule { Date = baseDate.AddDays(1), Content = "前往四姑娘山徒步", Hotel = "四姑娘山特色客栈", Meal = "藏式火锅", Note = "注意高原反应" },
                        new TripSchedule { Date = baseDate.AddDays(2), Content = "翻越夹金山前往丹巴", Hotel = "甲居藏寨民宿", Meal = "藏家宴", Note = "体验藏族歌舞" },
                        new TripSchedule { Date = baseDate.AddDays(4), Content = "返程总结，午餐后返回成都", Hotel = string.Empty, Meal = "回程简餐", Note = "行程结束" },
                    }),
                CreateTrip(
                    title: "海南环岛轻奢之旅",
                    organizerUid: userUids[2],
                    startDate: baseDate.AddDays(10),
                    endDate: baseDate.AddDays(15),
                    startLocation: "海口",
                    endLocation: "三亚",
                    description: "海口出发环岛自驾，精选海景酒店与特色美食。",
                    status: TripStatuses.Planning,
                    expectedParticipants: 16,
                    coverImageUrl: "https://images.unsplash.com/photo-1507525428034-b723cf961d3e",
                    startLatitude: 20.0440,
                    startLongitude: 110.1999,
                    endLatitude: 18.2528,
                    endLongitude: 109.5119,
                    routes: new[]
                    {
                        new TripRoute { OrderIndex = 1, Name = "海口集合", Latitude = 20.0440, Longitude = 110.1999, Description = "交车&欢迎晚宴" },
                        new TripRoute { OrderIndex = 2, Name = "博鳌东屿岛", Latitude = 19.1480, Longitude = 110.5093, Description = "五星级温泉度假" },
                        new TripRoute { OrderIndex = 3, Name = "万宁日月湾", Latitude = 18.7995, Longitude = 110.4006, Description = "冲浪体验" },
                        new TripRoute { OrderIndex = 4, Name = "三亚亚龙湾", Latitude = 18.2293, Longitude = 109.5220, Description = "海景酒店休闲" },
                    },
                    schedules: new[]
                    {
                        new TripSchedule { Date = baseDate.AddDays(10), Content = "海口集合交车", Hotel = "海口香格里拉", Meal = "欢迎自助餐", Note = "领取行程礼包" },
                        new TripSchedule { Date = baseDate.AddDays(11), Content = "前往博鳌入住度假酒店", Hotel = "博鳌金海岸", Meal = "海鲜盛宴", Note = "晚间泡温泉" },
                        new TripSchedule { Date = baseDate.AddDays(12), Content = "经万宁至陵水", Hotel = "清水湾假日酒店", Meal = "黎家风味", Note = "安排冲浪课程" },
                        new TripSchedule { Date = baseDate.AddDays(14), Content = "抵达三亚自由活动", Hotel = "三亚艾迪逊酒店", Meal = "海边烧烤", Note = "可选深潜活动" },
                    }),
                CreateTrip(
                    title: "北疆秋色摄影行",
                    organizerUid: userUids[3],
                    startDate: baseDate.AddDays(20),
                    endDate: baseDate.AddDays(27),
                    startLocation: "乌鲁木齐",
                    endLocation: "乌鲁木齐",
                    description: "秋日北疆摄影专线，阿勒泰至喀纳斯金秋美景。",
                    status: TripStatuses.Published,
                    expectedParticipants: 10,
                    coverImageUrl: "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                    startLatitude: 43.8256,
                    startLongitude: 87.6168,
                    endLatitude: 43.8256,
                    endLongitude: 87.6168,
                    routes: new[]
                    {
                        new TripRoute { OrderIndex = 1, Name = "乌鲁木齐集合", Latitude = 43.8256, Longitude = 87.6168, Description = "越野车调试" },
                        new TripRoute { OrderIndex = 2, Name = "可可托海", Latitude = 47.2011, Longitude = 89.5722, Description = "峡谷日落摄影" },
                        new TripRoute { OrderIndex = 3, Name = "禾木村", Latitude = 47.0196, Longitude = 86.4189, Description = "日出拍摄白桦林" },
                        new TripRoute { OrderIndex = 4, Name = "喀纳斯湖", Latitude = 48.9980, Longitude = 87.3650, Description = "金色湖畔徒步" },
                        new TripRoute { OrderIndex = 5, Name = "乌鲁木齐返程", Latitude = 43.8256, Longitude = 87.6168, Description = "分享交流" },
                    },
                    schedules: new[]
                    {
                        new TripSchedule { Date = baseDate.AddDays(20), Content = "乌鲁木齐集合办理手续", Hotel = "乌鲁木齐君悦酒店", Meal = "新疆手抓饭", Note = "讲解安全事项" },
                        new TripSchedule { Date = baseDate.AddDays(21), Content = "驱车前往可可托海", Hotel = "可可托海度假山庄", Meal = "哈萨克烤全羊", Note = "傍晚拍摄日落" },
                        new TripSchedule { Date = baseDate.AddDays(23), Content = "途经布尔津到禾木", Hotel = "禾木小木屋", Meal = "图瓦家宴", Note = "安排马队体验" },
                        new TripSchedule { Date = baseDate.AddDays(25), Content = "喀纳斯湖自由创作", Hotel = "喀纳斯宾馆", Meal = "当地特色餐", Note = "夜间星空摄影" },
                        new TripSchedule { Date = baseDate.AddDays(27), Content = "返程总结分享", Hotel = string.Empty, Meal = "送机早餐", Note = "行程圆满结束" },
                    })
            };

            context.Trips.AddRange(seededTrips);
            context.SaveChanges();

            var organizerUids = seededTrips
                .Select(t => t.OrganizerUid)
                .Distinct(StringComparer.Ordinal)
                .ToList();
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

        var rolesByKey = context.Roles
            .Where(role => role.Key == RoleKeys.User || role.Key == RoleKeys.Admin)
            .ToDictionary(role => role.Key);

        if (rolesByKey.Count > 0)
        {
            var adminUid = userUids.FirstOrDefault();
            var distinctUserUids = userUids.Distinct(StringComparer.Ordinal).ToArray();

            var requestedAssignments = new List<UserRoleAssignment>();

            if (rolesByKey.TryGetValue(RoleKeys.User, out var userRole))
            {
                var userRoleGrantedAt = DateTime.UtcNow;

                requestedAssignments.AddRange(distinctUserUids.Select(uid => new UserRoleAssignment
                {
                    UserUid = uid,
                    RoleId = userRole.Id,
                    GrantedAt = userRoleGrantedAt
                }));
            }

            if (!string.IsNullOrEmpty(adminUid) && rolesByKey.TryGetValue(RoleKeys.Admin, out var adminRole))
            {
                requestedAssignments.Add(new UserRoleAssignment
                {
                    UserUid = adminUid,
                    RoleId = adminRole.Id,
                    GrantedAt = DateTime.UtcNow
                });
            }

            var assignments = requestedAssignments
                .GroupBy(assignment => new { assignment.UserUid, assignment.RoleId })
                .Select(group => group.OrderBy(a => a.GrantedAt).First())
                .ToList();

            if (assignments.Count > 0)
            {
                var assignmentUsers = assignments
                    .Select(assignment => assignment.UserUid)
                    .Distinct()
                    .ToList();
                var assignmentRoles = assignments
                    .Select(assignment => assignment.RoleId)
                    .Distinct()
                    .ToList();

                var existingAssignments = context.UserRoles
                    .Where(assignment => assignmentUsers.Contains(assignment.UserUid) && assignmentRoles.Contains(assignment.RoleId))
                    .Select(assignment => new { assignment.UserUid, assignment.RoleId })
                    .ToList()
                    .Select(assignment => (assignment.UserUid, assignment.RoleId))
                    .ToHashSet();

                var newAssignments = assignments
                    .Where(assignment => !existingAssignments.Contains((assignment.UserUid, assignment.RoleId)))
                    .ToList();

                if (newAssignments.Count > 0)
                {
                    context.UserRoles.AddRange(newAssignments);
                    context.SaveChanges();
                }
            }
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

    private static Trip CreateTrip(
        string title,
        string organizerUid,
        DateTime startDate,
        DateTime endDate,
        string startLocation,
        string endLocation,
        string description,
        string status,
        int expectedParticipants,
        string coverImageUrl,
        double? startLatitude,
        double? startLongitude,
        double? endLatitude,
        double? endLongitude,
        IEnumerable<TripRoute> routes,
        IEnumerable<TripSchedule> schedules)
    {
        var normalizedStatus = !string.IsNullOrWhiteSpace(status) && TripStatuses.IsValid(status)
            ? status.Trim()
            : TripStatuses.Planning;

        if (endDate < startDate)
        {
            endDate = startDate;
        }

        var createdAt = startDate.AddDays(-7);
        var lastUpdated = createdAt.AddDays(2);
        if (lastUpdated < createdAt)
        {
            lastUpdated = createdAt;
        }

        return new Trip
        {
            Title = title,
            OrganizerUid = organizerUid,
            StartDate = startDate,
            EndDate = endDate,
            StartLocation = startLocation,
            EndLocation = endLocation,
            Description = description,
            Status = normalizedStatus,
            ExpectedParticipants = Math.Max(0, expectedParticipants),
            CreatedAt = createdAt,
            LastUpdated = lastUpdated,
            StartLatitude = startLatitude,
            StartLongitude = startLongitude,
            EndLatitude = endLatitude,
            EndLongitude = endLongitude,
            CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? string.Empty : coverImageUrl.Trim(),
            Routes = routes?.OrderBy(r => r.OrderIndex).Select(r => new TripRoute
            {
                OrderIndex = r.OrderIndex,
                Name = r.Name,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Description = r.Description
            }).ToList() ?? new List<TripRoute>(),
            Schedules = schedules?.OrderBy(s => s.Date).Select(s => new TripSchedule
            {
                Date = s.Date,
                Content = s.Content,
                Hotel = s.Hotel,
                Meal = s.Meal,
                Note = s.Note
            }).ToList() ?? new List<TripSchedule>(),
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
            IdentityLabel = UserIdentityLabels.Visitor,
            CreatedAt = DateTime.UtcNow
        };
    }
}
