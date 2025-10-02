using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Crew.Api.Models.RemoteConfig;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.FirebaseRemoteConfig.v1;
using Google.Apis.FirebaseRemoteConfig.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crew.Api.Services;

public class FirebaseAdminService : IFirebaseAdminService
{
    private const string DefaultDisclaimersParameterKey = "disclaimers";

    private readonly ILogger<FirebaseAdminService> _logger;
    private readonly GoogleCredential? _credential;
    private readonly string? _projectId;
    private readonly string _disclaimersParameterKey;
    private readonly Lazy<FirebaseRemoteConfigService?> _remoteConfigService;
    private readonly JsonSerializerOptions _serializerOptions;

    public FirebaseAdminService(IConfiguration configuration, ILogger<FirebaseAdminService> logger)
    {
        _logger = logger;
        _credential = LoadCredential(configuration);
        _projectId = configuration["Firebase:ProjectId"];
        _disclaimersParameterKey = configuration["Firebase:RemoteConfig:DisclaimerParameterKey"] ?? DefaultDisclaimersParameterKey;

        EnsureFirebaseApp(_credential);

        _remoteConfigService = new Lazy<FirebaseRemoteConfigService?>(CreateRemoteConfigService, LazyThreadSafetyMode.ExecutionAndPublication);
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
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

    public async Task<IReadOnlyList<RemoteConfigDisclaimerDto>> GetDisclaimersAsync(CancellationToken cancellationToken = default)
    {
        var template = await GetRemoteConfigTemplateAsync(cancellationToken);
        return ParseDisclaimers(template);
    }

    public async Task<RemoteConfigDisclaimerDto?> GetDisclaimerAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var disclaimers = await GetDisclaimersAsync(cancellationToken);
        return disclaimers.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<RemoteConfigDisclaimerDto> CreateDisclaimerAsync(CreateRemoteConfigDisclaimerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = await GetRemoteConfigTemplateAsync(cancellationToken);
        var disclaimers = ParseDisclaimers(template).ToList();

        var disclaimer = new RemoteConfigDisclaimerDto
        {
            Id = Guid.NewGuid().ToString("N"),
            Message = NormalizeMessage(request.Message),
            Locale = NormalizeLocale(request.Locale),
            LastUpdatedUtc = DateTime.UtcNow,
        };

        disclaimers.Add(disclaimer);
        await SaveDisclaimersAsync(template, disclaimers, cancellationToken);
        return disclaimer;
    }

    public async Task<RemoteConfigDisclaimerDto> UpdateDisclaimerAsync(string id, UpdateRemoteConfigDisclaimerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(request);

        var template = await GetRemoteConfigTemplateAsync(cancellationToken);
        var disclaimers = ParseDisclaimers(template).ToList();
        var index = disclaimers.FindIndex(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            throw new KeyNotFoundException($"Disclaimer '{id}' was not found in Remote Config.");
        }

        var updated = disclaimers[index] with
        {
            Message = NormalizeMessage(request.Message),
            Locale = NormalizeLocale(request.Locale),
            LastUpdatedUtc = DateTime.UtcNow,
        };

        disclaimers[index] = updated;
        await SaveDisclaimersAsync(template, disclaimers, cancellationToken);
        return updated;
    }

    public async Task<bool> DeleteDisclaimerAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var template = await GetRemoteConfigTemplateAsync(cancellationToken);
        var disclaimers = ParseDisclaimers(template).ToList();
        var removed = disclaimers.RemoveAll(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));

        if (removed <= 0)
        {
            return false;
        }

        await SaveDisclaimersAsync(template, disclaimers, cancellationToken);
        return true;
    }

    private GoogleCredential? LoadCredential(IConfiguration configuration)
    {
        var credentialsJson = configuration["Firebase:CredentialsJson"];
        var credentialsPath = configuration["Firebase:CredentialsPath"];

        try
        {
            if (!string.IsNullOrWhiteSpace(credentialsJson))
            {
                return GoogleCredential.FromJson(credentialsJson);
            }

            if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
            {
                return GoogleCredential.FromFile(credentialsPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Firebase credentials.");
        }

        if (string.IsNullOrWhiteSpace(credentialsJson) && string.IsNullOrWhiteSpace(credentialsPath))
        {
            _logger.LogWarning("Firebase credentials are not configured. Remote Config operations will be disabled.");
        }

        return null;
    }

    private static void EnsureFirebaseApp(GoogleCredential? credential)
    {
        if (FirebaseApp.DefaultInstance != null || credential is null)
        {
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
        });
    }

    private FirebaseRemoteConfigService? CreateRemoteConfigService()
    {
        if (_credential is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_projectId))
        {
            _logger.LogWarning("Firebase:ProjectId configuration is missing; Remote Config operations are disabled.");
            return null;
        }

        try
        {
            const string firebaseScope = FirebaseRemoteConfigService.ScopeConstants.Firebase;
            return new FirebaseRemoteConfigService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential.CreateScoped(firebaseScope),
                ApplicationName = "Crew.Api Remote Config",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Firebase Remote Config client.");
            return null;
        }
    }

    private FirebaseRemoteConfigService GetRemoteConfigServiceOrThrow()
    {
        var service = _remoteConfigService.Value;
        if (service is null)
        {
            throw new InvalidOperationException("Firebase Remote Config service is not configured. Ensure Firebase credentials and project ID are provided.");
        }

        return service;
    }

    private string GetProjectConfigResourceName()
    {
        if (string.IsNullOrWhiteSpace(_projectId))
        {
            throw new InvalidOperationException("Firebase:ProjectId configuration is required for Remote Config operations.");
        }

        return $"projects/{_projectId}/remoteConfig";
    }

    private async Task<RemoteConfig> GetRemoteConfigTemplateAsync(CancellationToken cancellationToken)
    {
        var service = GetRemoteConfigServiceOrThrow();
        var request = service.Projects.GetRemoteConfig(GetProjectConfigResourceName());
        return await request.ExecuteAsync(cancellationToken);
    }

    private IReadOnlyList<RemoteConfigDisclaimerDto> ParseDisclaimers(RemoteConfig template)
    {
        if (template.Parameters != null &&
            template.Parameters.TryGetValue(_disclaimersParameterKey, out var parameter) &&
            parameter?.DefaultValue?.Value is string json &&
            !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var disclaimers = JsonSerializer.Deserialize<List<RemoteConfigDisclaimerDto>>(json, _serializerOptions) ?? new List<RemoteConfigDisclaimerDto>();
                return disclaimers
                    .Where(d => !string.IsNullOrWhiteSpace(d.Id) && !string.IsNullOrWhiteSpace(d.Message))
                    .ToList();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Remote Config disclaimers from parameter {ParameterKey}.", _disclaimersParameterKey);
            }
        }

        return new List<RemoteConfigDisclaimerDto>();
    }

    private async Task SaveDisclaimersAsync(RemoteConfig template, IEnumerable<RemoteConfigDisclaimerDto> disclaimers, CancellationToken cancellationToken)
    {
        template.Parameters ??= new Dictionary<string, RemoteConfigParameter>();

        var ordered = disclaimers
            .OrderByDescending(d => d.LastUpdatedUtc)
            .ToList();

        template.Parameters[_disclaimersParameterKey] = new RemoteConfigParameter
        {
            DefaultValue = new RemoteConfigParameterValue
            {
                Value = JsonSerializer.Serialize(ordered, _serializerOptions),
            },
            Description = "Managed disclaimers for the Crew application.",
        };

        var service = GetRemoteConfigServiceOrThrow();
        var request = service.Projects.UpdateRemoteConfig(template, GetProjectConfigResourceName());
        request.IfMatch = string.IsNullOrEmpty(template.ETag) ? "*" : template.ETag;
        await request.ExecuteAsync(cancellationToken);
    }

    private static string NormalizeMessage(string message)
    {
        return string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim();
    }

    private static string? NormalizeLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return null;
        }

        return locale.Trim();
    }
}
