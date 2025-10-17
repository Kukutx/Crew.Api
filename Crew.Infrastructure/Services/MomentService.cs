using Crew.Application.Abstractions;
using Crew.Application.Moments;
using Crew.Domain.Entities;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Crew.Infrastructure.Services;

internal sealed class MomentService : IMomentService
{
    private readonly AppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public MomentService(AppDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MomentSummary>> SearchAsync(string? country, string? city, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Moments
            .Include(m => m.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(country))
        {
            var normalizedCountry = country.Trim();
            query = query.Where(m => EF.Functions.ILike(m.Country, normalizedCountry));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim();
            query = query.Where(m => m.City != null && EF.Functions.ILike(m.City!, normalizedCity));
        }

        var moments = await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MomentSummary(
                m.Id,
                m.UserId,
                m.User.DisplayName,
                m.Title,
                m.CoverImageUrl,
                m.Country,
                m.City,
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        return moments;
    }

    public async Task<IReadOnlyList<MomentSummary>> GetUserMomentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Moments
            .Include(m => m.User)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MomentSummary(
                m.Id,
                m.UserId,
                m.User.DisplayName,
                m.Title,
                m.CoverImageUrl,
                m.Country,
                m.City,
                m.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<MomentDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var moment = await _dbContext.Moments
            .Include(m => m.User)
            .Include(m => m.Images)
            .Include(m => m.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (moment is null)
        {
            return null;
        }

        return MapToDetail(moment);
    }

    public async Task<MomentDetail> CreateAsync(CreateMomentRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new InvalidOperationException("User not found");
        }

        if (request.EventId.HasValue)
        {
            var eventExists = await _dbContext.RoadTripEvents.AnyAsync(e => e.Id == request.EventId.Value, cancellationToken);
            if (!eventExists)
            {
                throw new InvalidOperationException("Event not found");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            throw new InvalidOperationException("Cover image is required.");
        }

        var moment = new Moment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            EventId = request.EventId,
            Title = request.Title.Trim(),
            Content = request.Content?.Trim(),
            CoverImageUrl = request.CoverImageUrl.Trim(),
            Country = request.Country.Trim(),
            City = request.City?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var (imageUrl, index) in request.Images.Select((url, index) => (url, index)))
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                continue;
            }

            moment.Images.Add(new MomentImage
            {
                Id = Guid.NewGuid(),
                Url = imageUrl.Trim(),
                SortOrder = index
            });
        }

        await _dbContext.Moments.AddAsync(moment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetAsync(moment.Id, cancellationToken))!;
    }

    public async Task<MomentCommentModel> AddCommentAsync(Guid momentId, Guid authorId, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Content is required.");
        }

        var moment = await _dbContext.Moments.FirstOrDefaultAsync(m => m.Id == momentId, cancellationToken)
            ?? throw new InvalidOperationException("Moment not found");

        var author = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == authorId, cancellationToken)
            ?? throw new InvalidOperationException("Author not found");

        var comment = new MomentComment
        {
            Id = Guid.NewGuid(),
            MomentId = moment.Id,
            AuthorId = author.Id,
            Content = content.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.MomentComments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MomentCommentModel(
            comment.Id,
            comment.AuthorId,
            author.DisplayName,
            comment.Content,
            comment.CreatedAt);
    }

    private static MomentDetail MapToDetail(Moment moment)
    {
        var images = moment.Images
            .OrderBy(i => i.SortOrder)
            .Select(i => i.Url)
            .ToList();

        var comments = moment.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new MomentCommentModel(
                c.Id,
                c.AuthorId,
                c.Author?.DisplayName,
                c.Content,
                c.CreatedAt))
            .ToList();

        return new MomentDetail(
            moment.Id,
            moment.UserId,
            moment.User?.DisplayName,
            moment.Title,
            moment.Content,
            moment.CoverImageUrl,
            moment.Country,
            moment.City,
            moment.CreatedAt,
            images,
            comments);
    }
}
