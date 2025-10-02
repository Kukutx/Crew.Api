using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crew.Api.Configuration;
using Crew.Api.Data.DbContexts;
using Crew.Api.Models;
using Crew.Api.Models.Authentication;
using Crew.Api.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crew.Api.Utils;

public class InitialAdminSeeder
{
    private const string FirebaseSignupEndpoint = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={0}";
    private const string FirebaseVerifyPasswordEndpoint = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={0}";
    private const string FirebaseUpdateEndpoint = "https://identitytoolkit.googleapis.com/v1/accounts:update?key={0}";

    private readonly AppDbContext _dbContext;
    private readonly IFirebaseAdminService _firebaseAdminService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InitialAdminSeeder> _logger;
    private readonly InitialAdminOptions _options;
    private readonly string? _firebaseApiKey;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public InitialAdminSeeder(
        AppDbContext dbContext,
        IFirebaseAdminService firebaseAdminService,
        IHttpClientFactory httpClientFactory,
        IOptions<InitialAdminOptions> options,
        IConfiguration configuration,
        ILogger<InitialAdminSeeder> logger)
    {
        _dbContext = dbContext;
        _firebaseAdminService = firebaseAdminService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
        _firebaseApiKey = configuration["Firebase:ApiKey"];
    }

    public async Task EnsureInitialAdminAsync(CancellationToken cancellationToken = default)
    {
        var email = _options.Email?.Trim();
        var password = _options.Password?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("Initial admin configuration is missing email or password; skipping admin seeding.");
            return;
        }

        var displayName = string.IsNullOrWhiteSpace(_options.DisplayName)
            ? "Administrator"
            : _options.DisplayName!.Trim();

        var desiredUid = string.IsNullOrWhiteSpace(_options.Uid) ? null : _options.Uid.Trim();

        var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Key == RoleKeys.Admin, cancellationToken);
        if (adminRole is null)
        {
            _logger.LogWarning("Admin role is not configured; skipping initial admin creation.");
            return;
        }

        var firebaseUid = await EnsureFirebaseAccountAsync(desiredUid, email, password, displayName, cancellationToken);
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            _logger.LogWarning("Failed to provision Firebase account for the initial admin; skipping database setup.");
            return;
        }

        var user = await _dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Uid == firebaseUid, cancellationToken);

        if (user is null && !string.IsNullOrWhiteSpace(desiredUid) && desiredUid != firebaseUid)
        {
            user = await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Uid == desiredUid, cancellationToken);
        }

        if (user is null)
        {
            user = await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        if (user is null)
        {
            user = new UserAccount
            {
                Uid = firebaseUid,
                Email = email,
                UserName = email,
                DisplayName = displayName,
                AvatarUrl = AvatarDefaults.FallbackUrl,
                CreatedAt = DateTime.UtcNow,
                Status = UserStatuses.Active,
            };

            user.Roles.Add(new UserRoleAssignment
            {
                RoleId = adminRole.Id,
                UserUid = firebaseUid,
                GrantedAt = DateTime.UtcNow,
            });

            _dbContext.Users.Add(user);
        }
        else
        {
            if (user.Uid != firebaseUid)
            {
                _logger.LogWarning(
                    "Initial admin Firebase UID {FirebaseUid} does not match existing user UID {UserUid}. Keeping existing UID.",
                    firebaseUid,
                    user.Uid);
                firebaseUid = user.Uid;
            }

            user.Email = email;
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                user.UserName = email;
            }

            if (string.IsNullOrWhiteSpace(user.DisplayName) || user.DisplayName.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.DisplayName = displayName;
            }

            if (!user.Roles.Any(r => r.RoleId == adminRole.Id))
            {
                user.Roles.Add(new UserRoleAssignment
                {
                    RoleId = adminRole.Id,
                    UserUid = user.Uid,
                    GrantedAt = DateTime.UtcNow,
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _firebaseAdminService.SetAdminClaimAsync(firebaseUid, true, cancellationToken);
    }

    private async Task<string?> EnsureFirebaseAccountAsync(
        string? desiredUid,
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken)
    {
        if (FirebaseApp.DefaultInstance != null)
        {
            return await EnsureWithAdminSdkAsync(desiredUid, email, password, displayName, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(_firebaseApiKey))
        {
            _logger.LogWarning("Firebase API key is missing; cannot create the initial admin account.");
            return null;
        }

        var client = _httpClientFactory.CreateClient();

        var signUpUri = string.Format(CultureInfo.InvariantCulture, FirebaseSignupEndpoint, _firebaseApiKey);
        var request = new FirebaseSignupRequest(email, password);
        using var response = await client.PostAsJsonAsync(signUpUri, request, _serializerOptions, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadFromJsonAsync<GoogleToken>(_serializerOptions, cancellationToken);
            if (token is not null)
            {
                await EnsureDisplayNameAsync(client, token.idToken, displayName, cancellationToken);
                return token.localId;
            }

            _logger.LogWarning("Received empty response when creating the initial admin with Firebase REST API.");
            return null;
        }

        var errorPayload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!errorPayload.Contains("EMAIL_EXISTS", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Failed to create the initial admin via Firebase REST API. Response: {Response}", errorPayload);
            return null;
        }

        var verifyUri = string.Format(CultureInfo.InvariantCulture, FirebaseVerifyPasswordEndpoint, _firebaseApiKey);
        var loginRequest = new FireBaseLoginInfo { Email = email, Password = password };
        using var loginResponse = await client.PostAsJsonAsync(verifyUri, loginRequest, _serializerOptions, cancellationToken);
        if (!loginResponse.IsSuccessStatusCode)
        {
            var loginPayload = await loginResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Initial admin account already exists in Firebase but the configured password could not be verified. Response: {Response}",
                loginPayload);
            return null;
        }

        var loginToken = await loginResponse.Content.ReadFromJsonAsync<GoogleToken>(_serializerOptions, cancellationToken);
        if (loginToken is null)
        {
            _logger.LogWarning("Failed to parse Firebase login response when verifying the initial admin account.");
            return null;
        }

        await EnsureDisplayNameAsync(client, loginToken.idToken, displayName, cancellationToken);
        return loginToken.localId;
    }

    private async Task<string?> EnsureWithAdminSdkAsync(
        string? desiredUid,
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken)
    {
        UserRecord? userRecord = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(desiredUid))
            {
                userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(desiredUid, cancellationToken);
            }
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            userRecord = null;
        }

        if (userRecord is null)
        {
            try
            {
                userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, cancellationToken);
            }
            catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                userRecord = null;
            }
        }

        if (userRecord is null)
        {
            var args = new UserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = displayName,
                EmailVerified = true,
            };

            if (!string.IsNullOrWhiteSpace(desiredUid))
            {
                args.Uid = desiredUid;
            }

            userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args, cancellationToken);
            return userRecord.Uid;
        }

        var updateArgs = new UserRecordArgs
        {
            Uid = userRecord.Uid,
            Email = email,
            Password = password,
            DisplayName = displayName,
            EmailVerified = true,
        };

        var updatedUser = await FirebaseAuth.DefaultInstance.UpdateUserAsync(updateArgs, cancellationToken);
        return updatedUser.Uid;
    }

    private async Task EnsureDisplayNameAsync(HttpClient client, string idToken, string displayName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_firebaseApiKey) || string.IsNullOrWhiteSpace(displayName))
        {
            return;
        }

        var updateUri = string.Format(CultureInfo.InvariantCulture, FirebaseUpdateEndpoint, _firebaseApiKey);
        var payload = new FirebaseUpdateRequest(idToken, displayName);
        using var response = await client.PostAsJsonAsync(updateUri, payload, _serializerOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to update Firebase display name for the initial admin. Response: {Response}", body);
        }
    }

    private sealed record FirebaseSignupRequest(string Email, string Password)
    {
        public bool ReturnSecureToken { get; } = true;
    }

    private sealed record FirebaseUpdateRequest(string IdToken, string DisplayName)
    {
        public bool ReturnSecureToken { get; } = true;
    }
}
