using System;
using System.Globalization;
using System.Text;

namespace Crew.Application.Events;

public readonly record struct FeedCursorValue(DateTimeOffset CreatedAt, double DistanceKm, double Engagement, Guid EventId);

public static class FeedCursor
{
    public static bool TryParse(string? cursor, out FeedCursorValue value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return false;
        }

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4)
            {
                return false;
            }

            if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks))
            {
                return false;
            }

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var distanceKm))
            {
                return false;
            }

            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var engagement))
            {
                return false;
            }

            if (!Guid.TryParse(parts[3], out var id))
            {
                return false;
            }

            value = new FeedCursorValue(new DateTimeOffset(ticks, TimeSpan.Zero), distanceKm, engagement, id);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string Encode(EventCard card)
        => Encode(card.CreatedAt, card.DistanceKm, card.Engagement, card.Id);

    public static string Encode(DateTimeOffset createdAt, double distanceKm, double engagement, Guid id)
    {
        var payload = FormattableString.Invariant($"{createdAt.UtcTicks}:{distanceKm:R}:{engagement:R}:{id}");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }
}
