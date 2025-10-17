namespace Crew.Application.Moments;

public interface IMomentService
{
    Task<IReadOnlyList<MomentSummary>> SearchAsync(string? country, string? city, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MomentSummary>> GetUserMomentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MomentDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MomentDetail> CreateAsync(CreateMomentRequest request, CancellationToken cancellationToken = default);
    Task<MomentCommentModel> AddCommentAsync(Guid momentId, Guid authorId, string content, CancellationToken cancellationToken = default);
}
