using Crew.Application.Moments;
using Crew.Contracts.Moments;
using System.Linq;

namespace Crew.Api.Mapping;

public static class MomentMappings
{
    public static MomentSummaryDto ToDto(this MomentSummary summary)
    {
        return new MomentSummaryDto(
            summary.Id,
            summary.UserId,
            summary.UserDisplayName,
            summary.Title,
            summary.CoverImageUrl,
            summary.Country,
            summary.City,
            summary.CreatedAt);
    }

    public static MomentDetailDto ToDto(this MomentDetail detail)
    {
        return new MomentDetailDto(
            detail.Id,
            detail.UserId,
            detail.UserDisplayName,
            detail.Title,
            detail.Content,
            detail.CoverImageUrl,
            detail.Country,
            detail.City,
            detail.CreatedAt,
            detail.Images,
            detail.Comments.Select(ToDto).ToList());
    }

    public static MomentCommentDto ToDto(this MomentCommentModel comment)
    {
        return new MomentCommentDto(
            comment.Id,
            comment.AuthorId,
            comment.AuthorDisplayName,
            comment.Content,
            comment.CreatedAt);
    }
}
