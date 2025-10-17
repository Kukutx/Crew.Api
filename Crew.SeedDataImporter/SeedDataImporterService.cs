using ClosedXML.Excel;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Crew.SeedDataImporter.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Crew.SeedDataImporter;

public sealed class SeedDataImporterService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SeedDataImporterService> _logger;
    private readonly IOptionsMonitor<SeedOptions> _optionsMonitor;
    private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public SeedDataImporterService(
        AppDbContext dbContext,
        ILogger<SeedDataImporterService> logger,
        IOptionsMonitor<SeedOptions> optionsMonitor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _optionsMonitor = optionsMonitor;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue;
        if (string.IsNullOrWhiteSpace(options.ExcelPath))
        {
            throw new InvalidOperationException("Seed:ExcelPath must be configured.");
        }

        var excelPath = ResolveExcelPath(options.ExcelPath);

        if (!File.Exists(excelPath))
        {
            throw new FileNotFoundException($"Excel file not found at '{excelPath}'.", excelPath);
        }

        using var workbook = new XLWorkbook(excelPath);

        var now = DateTimeOffset.UtcNow;
        _logger.LogInformation(
           "Starting seed import from {Path} (overwrite existing: {Overwrite}, configured path: {Configured}).",
           excelPath,
           options.OverwriteExisting,
           options.ExcelPath);

        var users = await ImportUsersAsync(workbook, now, options.OverwriteExisting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tags = await ImportTagsAsync(workbook, now, options.OverwriteExisting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await ImportUserTagsAsync(workbook, users, tags, now, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var events = await ImportEventsAsync(workbook, users, now, options.OverwriteExisting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await ImportEventSegmentsAsync(workbook, events, options.OverwriteExisting, cancellationToken);
        await ImportEventTagsAsync(workbook, events, tags, now, cancellationToken);
        await ImportRegistrationsAsync(workbook, events, users, now, options.OverwriteExisting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, User>> ImportUsersAsync(XLWorkbook workbook, DateTimeOffset now, bool overwrite, CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "Users");
        if (worksheet is null)
        {
            _logger.LogWarning("No 'Users' worksheet found. Skipping user import.");
            return await _dbContext.Users.ToDictionaryAsync(x => x.FirebaseUid, cancellationToken);
        }

        var existing = await _dbContext.Users.ToDictionaryAsync(x => x.FirebaseUid, cancellationToken);
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var firebaseUid = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                continue;
            }

            var displayName = row.Cell(2).GetString();
            var email = row.Cell(3).GetString();
            var roleText = row.Cell(4).GetString();
            var bio = row.Cell(5).GetString();
            var avatarUrl = row.Cell(6).GetString();
            var createdAt = row.Cell(7).TryGetValue<DateTimeOffset>(out var created) ? created : now;

            var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            if (!existing.TryGetValue(firebaseUid, out var user))
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FirebaseUid = firebaseUid,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? firebaseUid : displayName,
                    Email = normalizedEmail,
                    Role = ParseEnum(roleText, UserRole.User),
                    Bio = string.IsNullOrWhiteSpace(bio) ? null : bio,
                    AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl,
                    CreatedAt = createdAt,
                    UpdatedAt = null
                };

                _dbContext.Users.Add(user);
                existing[firebaseUid] = user;
                _logger.LogInformation("Added user {User}", firebaseUid);
            }
            else if (overwrite)
            {
                user.DisplayName = string.IsNullOrWhiteSpace(displayName) ? user.DisplayName : displayName;
                user.Email = normalizedEmail;
                user.Role = ParseEnum(roleText, user.Role);
                user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;
                user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
                user.UpdatedAt = now;
                _logger.LogInformation("Updated user {User}", firebaseUid);
            }
        }

        return existing;
    }

    private async Task<Dictionary<string, Tag>> ImportTagsAsync(XLWorkbook workbook, DateTimeOffset now, bool overwrite, CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "Tags");
        if (worksheet is null)
        {
            _logger.LogWarning("No 'Tags' worksheet found. Skipping tag import.");
            return await _dbContext.Tags.ToDictionaryAsync(x => x.Name, cancellationToken);
        }

        var existing = await _dbContext.Tags.ToDictionaryAsync(x => x.Name, cancellationToken);
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var categoryText = row.Cell(2).GetString();
            var createdAt = row.Cell(3).TryGetValue<DateTimeOffset>(out var created) ? created : now;

            if (!existing.TryGetValue(name, out var tag))
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Category = ParseEnum(categoryText, TagCategory.General),
                    CreatedAt = createdAt
                };

                _dbContext.Tags.Add(tag);
                existing[name] = tag;
                _logger.LogInformation("Added tag {Tag}", name);
            }
            else if (overwrite)
            {
                tag.Category = ParseEnum(categoryText, tag.Category);
                tag.UpdatedAt = now;
                _logger.LogInformation("Updated tag {Tag}", name);
            }
        }

        return existing;
    }

    private async Task ImportUserTagsAsync(XLWorkbook workbook, IReadOnlyDictionary<string, User> users, IReadOnlyDictionary<string, Tag> tags, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "UserTags");
        if (worksheet is null)
        {
            _logger.LogInformation("No 'UserTags' worksheet found. Skipping user-tag links.");
            return;
        }

        var existingLinks = await _dbContext.UserTags
            .Select(x => new { x.UserId, x.TagId })
            .ToListAsync(cancellationToken);

        var existingSet = existingLinks
            .Select(x => (x.UserId, x.TagId))
            .ToHashSet();

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userKey = row.Cell(1).GetString().Trim();
            var tagKey = row.Cell(2).GetString().Trim();
            if (string.IsNullOrWhiteSpace(userKey) || string.IsNullOrWhiteSpace(tagKey))
            {
                continue;
            }

            if (!users.TryGetValue(userKey, out var user))
            {
                _logger.LogWarning("User {User} referenced in UserTags sheet was not found. Skipping.", userKey);
                continue;
            }

            if (!tags.TryGetValue(tagKey, out var tag))
            {
                _logger.LogWarning("Tag {Tag} referenced in UserTags sheet was not found. Skipping.", tagKey);
                continue;
            }

            if (existingSet.Contains((user.Id, tag.Id)))
            {
                continue;
            }

            _dbContext.UserTags.Add(new UserTag
            {
                UserId = user.Id,
                TagId = tag.Id,
                CreatedAt = now
            });

            existingSet.Add((user.Id, tag.Id));
            _logger.LogInformation("Linked user {User} with tag {Tag}.", userKey, tagKey);
        }
    }

    private async Task<Dictionary<string, RoadTripEvent>> ImportEventsAsync(
        XLWorkbook workbook,
        IReadOnlyDictionary<string, User> users,
        DateTimeOffset now,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "Events");
        if (worksheet is null)
        {
            _logger.LogWarning("No 'Events' worksheet found. Skipping events import.");
            return await _dbContext.RoadTripEvents.ToDictionaryAsync(x => x.Title, cancellationToken);
        }

        var existing = await _dbContext.RoadTripEvents.ToDictionaryAsync(x => x.Title, cancellationToken);
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var title = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var ownerKey = row.Cell(2).GetString().Trim();
            if (!users.TryGetValue(ownerKey, out var owner))
            {
                _logger.LogWarning("Owner {Owner} for event {Title} was not found. Skipping event.", ownerKey, title);
                continue;
            }

            var description = row.Cell(3).GetString();
            var startTime = row.Cell(4).TryGetValue<DateTimeOffset>(out var start) ? start : now;
            var endTime = row.Cell(5).TryGetValue<DateTimeOffset?>(out var end) ? end : null;
            var startLat = TryGetDouble(row.Cell(6));
            var startLng = TryGetDouble(row.Cell(7));
            var endLat = TryGetDouble(row.Cell(8));
            var endLng = TryGetDouble(row.Cell(9));
            var maxParticipants = row.Cell(10).TryGetValue<int?>(out var max) ? max : null;
            var visibilityText = row.Cell(11).GetString();
            var polyline = row.Cell(12).GetString();

            if (startLat is null || startLng is null)
            {
                _logger.LogWarning("Event {Title} is missing start coordinates. Skipping.", title);
                continue;
            }

            var startPoint = _geometryFactory.CreatePoint(new Coordinate(startLng.Value, startLat.Value));
            Point? endPoint = null;
            if (endLat is not null && endLng is not null)
            {
                endPoint = _geometryFactory.CreatePoint(new Coordinate(endLng.Value, endLat.Value));
            }

            if (!existing.TryGetValue(title, out var roadTripEvent))
            {
                roadTripEvent = new RoadTripEvent
                {
                    Id = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Title = title,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    StartTime = startTime,
                    EndTime = endTime,
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    Location = startPoint,
                    RoutePolyline = string.IsNullOrWhiteSpace(polyline) ? null : polyline,
                    MaxParticipants = maxParticipants,
                    Visibility = ParseEnum(visibilityText, EventVisibility.Private),
                    CreatedAt = now
                };

                _dbContext.RoadTripEvents.Add(roadTripEvent);
                existing[title] = roadTripEvent;
                _logger.LogInformation("Added event {Title}.", title);
            }
            else if (overwrite)
            {
                roadTripEvent.OwnerId = owner.Id;
                roadTripEvent.Description = string.IsNullOrWhiteSpace(description) ? null : description;
                roadTripEvent.StartTime = startTime;
                roadTripEvent.EndTime = endTime;
                roadTripEvent.StartPoint = startPoint;
                roadTripEvent.EndPoint = endPoint;
                roadTripEvent.Location = startPoint;
                roadTripEvent.RoutePolyline = string.IsNullOrWhiteSpace(polyline) ? null : polyline;
                roadTripEvent.MaxParticipants = maxParticipants;
                roadTripEvent.Visibility = ParseEnum(visibilityText, roadTripEvent.Visibility);
                _logger.LogInformation("Updated event {Title}.", title);
            }
        }

        return existing;
    }

    private async Task ImportEventSegmentsAsync(
        XLWorkbook workbook,
        IReadOnlyDictionary<string, RoadTripEvent> events,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "EventSegments");
        if (worksheet is null)
        {
            _logger.LogInformation("No 'EventSegments' worksheet found. Skipping segments import.");
            return;
        }

        var segmentsByEvent = new Dictionary<Guid, List<EventSegment>>();
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventTitle = row.Cell(1).GetString().Trim();
            if (!events.TryGetValue(eventTitle, out var roadTripEvent))
            {
                _logger.LogWarning("Event {Title} referenced in EventSegments sheet not found. Skipping row.", eventTitle);
                continue;
            }

            var sequence = row.Cell(2).TryGetValue<int>(out var seq) ? seq : (int?)null;
            var lat = TryGetDouble(row.Cell(3));
            var lng = TryGetDouble(row.Cell(4));
            var note = row.Cell(5).GetString();

            if (sequence is null || lat is null || lng is null)
            {
                _logger.LogWarning("Segment for event {Title} is missing required data. Skipping.", eventTitle);
                continue;
            }

            var segment = new EventSegment
            {
                Id = Guid.NewGuid(),
                EventId = roadTripEvent.Id,
                Seq = sequence.Value,
                Waypoint = _geometryFactory.CreatePoint(new Coordinate(lng.Value, lat.Value)),
                Note = string.IsNullOrWhiteSpace(note) ? null : note
            };

            if (!segmentsByEvent.TryGetValue(roadTripEvent.Id, out var list))
            {
                list = new List<EventSegment>();
                segmentsByEvent.Add(roadTripEvent.Id, list);
            }

            list.Add(segment);
        }

        foreach (var (eventId, segments) in segmentsByEvent)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (overwrite)
            {
                var existing = await _dbContext.EventSegments
                    .Where(x => x.EventId == eventId)
                    .ToListAsync(cancellationToken);
                _dbContext.EventSegments.RemoveRange(existing);
            }

            await _dbContext.EventSegments.AddRangeAsync(segments, cancellationToken);
            _logger.LogInformation("Added {Count} segments for event {EventId}.", segments.Count, eventId);
        }
    }

    private async Task ImportEventTagsAsync(
        XLWorkbook workbook,
        IReadOnlyDictionary<string, RoadTripEvent> events,
        IReadOnlyDictionary<string, Tag> tags,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "EventTags");
        if (worksheet is null)
        {
            _logger.LogInformation("No 'EventTags' worksheet found. Skipping event-tag links.");
            return;
        }

        var existing = await _dbContext.EventTags
            .Select(x => new { x.EventId, x.TagId })
            .ToListAsync(cancellationToken);

        var existingSet = existing.Select(x => (x.EventId, x.TagId)).ToHashSet();

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventTitle = row.Cell(1).GetString().Trim();
            var tagName = row.Cell(2).GetString().Trim();
            if (string.IsNullOrWhiteSpace(eventTitle) || string.IsNullOrWhiteSpace(tagName))
            {
                continue;
            }

            if (!events.TryGetValue(eventTitle, out var roadTripEvent))
            {
                _logger.LogWarning("Event {Title} referenced in EventTags sheet not found. Skipping row.", eventTitle);
                continue;
            }

            if (!tags.TryGetValue(tagName, out var tag))
            {
                _logger.LogWarning("Tag {Tag} referenced in EventTags sheet not found. Skipping row.", tagName);
                continue;
            }

            if (existingSet.Contains((roadTripEvent.Id, tag.Id)))
            {
                continue;
            }

            _dbContext.EventTags.Add(new EventTag
            {
                EventId = roadTripEvent.Id,
                TagId = tag.Id,
                CreatedAt = now
            });

            existingSet.Add((roadTripEvent.Id, tag.Id));
            _logger.LogInformation("Linked event {Event} with tag {Tag}.", eventTitle, tagName);
        }
    }

    private async Task ImportRegistrationsAsync(
        XLWorkbook workbook,
        IReadOnlyDictionary<string, RoadTripEvent> events,
        IReadOnlyDictionary<string, User> users,
        DateTimeOffset now,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var worksheet = TryGetWorksheet(workbook, "Registrations");
        if (worksheet is null)
        {
            _logger.LogInformation("No 'Registrations' worksheet found. Skipping registration import.");
            return;
        }

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventTitle = row.Cell(1).GetString().Trim();
            var userKey = row.Cell(2).GetString().Trim();
            var statusText = row.Cell(3).GetString();
            var createdCell = row.Cell(4);
            var createdAt = createdCell.TryGetValue<DateTimeOffset>(out var created) ? created : now;

            if (!events.TryGetValue(eventTitle, out var roadTripEvent))
            {
                _logger.LogWarning("Event {Event} referenced in Registrations sheet not found. Skipping row.", eventTitle);
                continue;
            }

            if (!users.TryGetValue(userKey, out var user))
            {
                _logger.LogWarning("User {User} referenced in Registrations sheet not found. Skipping row.", userKey);
                continue;
            }

            var registration = await _dbContext.Registrations
                .SingleOrDefaultAsync(x => x.EventId == roadTripEvent.Id && x.UserId == user.Id, cancellationToken);

            if (registration is null)
            {
                registration = new Registration
                {
                    Id = Guid.NewGuid(),
                    EventId = roadTripEvent.Id,
                    UserId = user.Id,
                    Status = ParseEnum(statusText, RegistrationStatus.Confirmed),
                    CreatedAt = createdAt
                };

                await _dbContext.Registrations.AddAsync(registration, cancellationToken);
                _logger.LogInformation("Added registration for user {User} to event {Event}.", userKey, eventTitle);
            }
            else if (overwrite)
            {
                registration.Status = ParseEnum(statusText, registration.Status);
                registration.CreatedAt = createdAt;
                _logger.LogInformation("Updated registration for user {User} to event {Event}.", userKey, eventTitle);
            }
        }
    }

    private static string ResolveExcelPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var baseDirectoryCandidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
        if (File.Exists(baseDirectoryCandidate))
        {
            return baseDirectoryCandidate;
        }

        var currentDirectoryCandidate = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
        if (File.Exists(currentDirectoryCandidate))
        {
            return currentDirectoryCandidate;
        }

        return baseDirectoryCandidate;
    }

    private static TEnum ParseEnum<TEnum>(string value, TEnum defaultValue)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : defaultValue;
    }

    private static IXLWorksheet? TryGetWorksheet(XLWorkbook workbook, string name)
    {
        return workbook.Worksheets
            .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static double? TryGetDouble(IXLCell cell)
    => cell.TryGetValue<double>(out var v) ? v : (double?)null;
}
