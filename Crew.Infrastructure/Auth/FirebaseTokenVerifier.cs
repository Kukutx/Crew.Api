using Crew.Application.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace Crew.Infrastructure.Auth;

internal sealed class FirebaseTokenVerifier : IFirebaseTokenVerifier
{
    private readonly Lazy<FirebaseApp> _app;

    public FirebaseTokenVerifier()
    {
        _app = new Lazy<FirebaseApp>(() => FirebaseApp.DefaultInstance ?? FirebaseApp.Create());
    }

    public async Task<FirebaseTokenResult?> VerifyAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            _ = _app.Value;
            var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token, cancellationToken);
            decoded.Claims.TryGetValue("name", out var nameClaim);
            decoded.Claims.TryGetValue("email", out var emailClaim);
            decoded.Claims.TryGetValue("role", out var roleClaim);

            return new FirebaseTokenResult(
                decoded.Uid,
                nameClaim as string,
                emailClaim as string,
                roleClaim?.ToString());
        }
        catch (FirebaseAuthException)
        {
            return null;
        }
    }
}
