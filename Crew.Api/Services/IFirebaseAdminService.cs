using Crew.Api.Models.RemoteConfig;

namespace Crew.Api.Services;

public interface IFirebaseAdminService
{
    Task SetAdminClaimAsync(string uid, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RemoteConfigDisclaimerDto>> GetDisclaimersAsync(CancellationToken cancellationToken = default);
    Task<RemoteConfigDisclaimerDto?> GetDisclaimerAsync(string id, CancellationToken cancellationToken = default);
    Task<RemoteConfigDisclaimerDto> CreateDisclaimerAsync(CreateRemoteConfigDisclaimerRequest request, CancellationToken cancellationToken = default);
    Task<RemoteConfigDisclaimerDto> UpdateDisclaimerAsync(string id, UpdateRemoteConfigDisclaimerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteDisclaimerAsync(string id, CancellationToken cancellationToken = default);
}
