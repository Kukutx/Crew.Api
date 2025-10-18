using System.Threading;
using System.Threading.Tasks;

namespace Crew.Application.Auth;

public interface IFirebaseTokenVerifier
{
    Task<FirebaseTokenResult?> VerifyAsync(string token, CancellationToken cancellationToken = default);
}

public sealed record FirebaseTokenResult(string FirebaseUid, string? DisplayName, string? Email, string? Role);
