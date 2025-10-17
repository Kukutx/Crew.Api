using Crew.Application.Users;
using Crew.Domain.Entities;
using Crew.Domain.Enums;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crew.Infrastructure.Services;

internal sealed class UserProfileCommandService : IUserProfileCommandService
{
    private readonly AppDbContext _dbContext;

    public UserProfileCommandService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, IEnumerable<string>? tags, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Tags)
                .ThenInclude(ut => ut.Tag)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found");

        if (bio != null)
        {
            user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        }

        if (avatarUrl != null)
        {
            user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        }

        if (tags is not null)
        {
            var normalized = tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Where(tag => tag.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingTagNames = user.Tags
                .Where(t => t.Tag is not null)
                .Select(t => t.Tag!.Name)
                .ToList();

            var toRemove = user.Tags
                .Where(ut => ut.Tag is null || !normalized.Contains(ut.Tag!.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var remove in toRemove)
            {
                _dbContext.UserTags.Remove(remove);
            }

            foreach (var tagName in normalized)
            {
                if (existingTagNames.Contains(tagName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var tag = await _dbContext.Tags
                    .FirstOrDefaultAsync(t => t.Category == TagCategory.User && t.Name.ToLower() == tagName.ToLower(), cancellationToken);

                if (tag is null)
                {
                    tag = new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = tagName,
                        Category = TagCategory.User,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _dbContext.Tags.AddAsync(tag, cancellationToken);
                }

                user.Tags.Add(new UserTag
                {
                    UserId = user.Id,
                    TagId = tag.Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Tag = tag
                });
            }
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
