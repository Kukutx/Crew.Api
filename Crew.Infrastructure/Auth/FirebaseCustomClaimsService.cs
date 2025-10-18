using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crew.Application.Auth;
using Crew.Domain.Enums;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace Crew.Infrastructure.Auth;

internal sealed class FirebaseCustomClaimsService : IFirebaseCustomClaimsService
{
    private readonly Lazy<FirebaseApp> _app;

    public FirebaseCustomClaimsService()
    {
        _app = new Lazy<FirebaseApp>(() => FirebaseApp.DefaultInstance ?? FirebaseApp.Create());
    }

    public async Task SetRoleAsync(string firebaseUid, UserRole role, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            throw new ArgumentException("Firebase UID must be provided.", nameof(firebaseUid));
        }

        cancellationToken.ThrowIfCancellationRequested();

        _ = _app.Value;

        var user = await FirebaseAuth.DefaultInstance.GetUserAsync(firebaseUid);
        var claims = user.CustomClaims is null
            ? new Dictionary<string, object>(StringComparer.Ordinal)
            : new Dictionary<string, object>(user.CustomClaims, StringComparer.Ordinal);

        claims["role"] = role.ToString();

        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(firebaseUid, claims);
    }
}
