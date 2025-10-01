using System.Collections.Generic;
using System.IO;
using Crew.Api.Configuration;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crew.Api.Services;

public class FirebaseAdminService : IFirebaseAdminService
{
    private readonly ILogger<FirebaseAdminService> _logger;
    private readonly FirebaseOptions _options;

    public FirebaseAdminService(IOptions<FirebaseOptions> options, ILogger<FirebaseAdminService> logger)
    {
        _logger = logger;
        _options = options.Value;
        EnsureFirebaseApp();
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

    private void EnsureFirebaseApp()
    {
        if (FirebaseApp.DefaultInstance != null)
        {
            return;
        }

        GoogleCredential? credential = null;
        if (!string.IsNullOrWhiteSpace(_options.CredentialsJson))
        {
            credential = GoogleCredential.FromJson(_options.CredentialsJson);
        }
        else if (!string.IsNullOrWhiteSpace(_options.CredentialsPath) && File.Exists(_options.CredentialsPath))
        {
            credential = GoogleCredential.FromFile(_options.CredentialsPath);
        }

        if (credential == null)
        {
            _logger.LogWarning("Firebase credentials are not configured. Skipping Firebase Admin initialization.");
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = string.IsNullOrWhiteSpace(_options.ProjectId) ? null : _options.ProjectId,
        });
    }
}
