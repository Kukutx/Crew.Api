namespace Crew.Api.Services;

public interface IFirebaseAdminService
{
    Task SetAdminClaimAsync(string uid, bool isAdmin, CancellationToken cancellationToken = default);

    Task EnsureUserAsync(
        string uid,
        string email,
        string displayName,
        string? password,
        CancellationToken cancellationToken = default);
}
