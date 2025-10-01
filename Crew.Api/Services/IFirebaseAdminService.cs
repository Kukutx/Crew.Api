namespace Crew.Api.Services;

public interface IFirebaseAdminService
{
    Task SetAdminClaimAsync(string uid, bool isAdmin, CancellationToken cancellationToken = default);
}
