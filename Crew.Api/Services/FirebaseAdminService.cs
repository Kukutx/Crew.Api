using System.Collections.Generic;
using System.IO;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crew.Api.Services;

public class FirebaseAdminService : IFirebaseAdminService
{
    private readonly ILogger<FirebaseAdminService> _logger;

    public FirebaseAdminService(IConfiguration configuration, ILogger<FirebaseAdminService> logger)
    {
        _logger = logger;
        EnsureFirebaseApp(configuration);
    }

    public async Task SetAdminClaimAsync(string uid, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            _logger.LogWarning("Firebase app is not configured; skipping custom claim update for {Uid}.", uid);
            return;
        }

        var claims = new Dictionary<string, object>
        {
            ["admin"] = isAdmin,
        };

        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims, cancellationToken);
    }

    private static void EnsureFirebaseApp(IConfiguration configuration)
    {
        if (FirebaseApp.DefaultInstance != null)
        {
            return;
        }

        var credentialsPath = configuration["Firebase:CredentialsPath"];
        var credentialsJson = configuration["Firebase:CredentialsJson"];

        GoogleCredential? credential = null;
        if (!string.IsNullOrWhiteSpace(credentialsJson))
        {
            credential = GoogleCredential.FromJson(credentialsJson);
        }
        else if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
        {
            credential = GoogleCredential.FromFile(credentialsPath);
        }

        if (credential == null)
        {
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
        });
    }
}
