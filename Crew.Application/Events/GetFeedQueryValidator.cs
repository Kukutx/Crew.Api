using FluentValidation;

namespace Crew.Application.Events;

public sealed class GetFeedQueryValidator : AbstractValidator<GetFeedQuery>
{
    public GetFeedQueryValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0)
            .LessThanOrEqualTo(200);

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);

        RuleFor(x => x.Cursor)
            .Must(cursor => string.IsNullOrWhiteSpace(cursor) || FeedCursor.TryParse(cursor, out _))
            .WithMessage("Cursor is invalid.");
    }
}
