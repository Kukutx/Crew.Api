using System;
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

    public async Task EnsureUserAsync(
        string uid,
        string email,
        string displayName,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            _logger.LogWarning("Firebase app is not configured; skipping user provisioning for {Uid}.", uid);
            return;
        }

        UserRecord? user = null;
        try
        {
            user = await FirebaseAuth.DefaultInstance.GetUserAsync(uid, cancellationToken);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            user = null;
        }

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "Seed admin password is not configured; cannot create Firebase user for {Email}.",
                    email);
                return;
            }

            var request = new UserRecordArgs
            {
                Uid = uid,
                Email = email,
                DisplayName = displayName,
                Password = password,
                EmailVerified = true,
                Disabled = false,
            };

            await FirebaseAuth.DefaultInstance.CreateUserAsync(request, cancellationToken);
            _logger.LogInformation("Created Firebase seed admin user {Email} ({Uid}).", email, uid);
            return;
        }

        var updateArgs = new UserRecordArgs
        {
            Uid = uid,
        };

        var needsUpdate = false;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            updateArgs.Email = email;
            needsUpdate = true;
        }

        if (!string.Equals(user.DisplayName, displayName, StringComparison.Ordinal))
        {
            updateArgs.DisplayName = displayName;
            needsUpdate = true;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            updateArgs.Password = password;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            await FirebaseAuth.DefaultInstance.UpdateUserAsync(updateArgs, cancellationToken);
            _logger.LogInformation("Updated Firebase seed admin user {Uid} to match configuration.", uid);
        }
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
