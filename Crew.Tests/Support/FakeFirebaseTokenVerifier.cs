using Crew.Application.Auth;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Crew.Tests.Support;

public sealed class FakeFirebaseTokenVerifier : IFirebaseTokenVerifier
{
    private readonly Dictionary<string, FirebaseTokenResult> _tokens = new(StringComparer.Ordinal);

    public Task<FirebaseTokenResult> VerifyAsync(string token, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tokens.TryGetValue(token, out var result) ? result : null);
    }

    public void SetToken(string token, FirebaseTokenResult result)
    {
        _tokens[token] = result;
    }

    /* Quota exceeded. Please try again later. */
    //public Task<FirebaseTokenResult> VerifyAsync(string token, CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}
}
