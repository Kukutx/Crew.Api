using Crew.Application.Auth;

namespace Crew.Tests.Support;

public sealed class FakeFirebaseTokenVerifier : IFirebaseTokenVerifier
{
    private readonly Dictionary<string, FirebaseTokenResult> _tokens = new(StringComparer.Ordinal);

    public Task<FirebaseTokenResult?> VerifyAsync(string token, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tokens.TryGetValue(token, out var result) ? result : null);
    }

    public void SetToken(string token, FirebaseTokenResult result)
    {
        _tokens[token] = result;
    }
}
