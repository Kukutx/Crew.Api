using System;

namespace Crew.Api.Models;

public record UserFollowSummary(
    string Uid,
    string UserName,
    string DisplayName,
    string AvatarUrl,
    DateTime FollowedAt);
