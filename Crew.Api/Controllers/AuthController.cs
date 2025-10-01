using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Crew.Api.Models;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crew.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthController> _logger;
    private readonly string _firebaseApiKey;

    public AuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _firebaseApiKey = configuration["Firebase:ApiKey"]
            ?? throw new InvalidOperationException("Firebase:ApiKey configuration is required");
    }

    [HttpPost]
    public async Task<ActionResult> GetToken([FromForm] LoginInfo loginInfo)
    {
        var uri = $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={_firebaseApiKey}";
        var client = _httpClientFactory.CreateClient();

        var fireBaseLoginInfo = new FireBaseLoginInfo
        {
            Email = loginInfo.Username,
            Password = loginInfo.Password
        };

        using var response = await client.PostAsJsonAsync(uri, fireBaseLoginInfo,
            new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (!response.IsSuccessStatusCode)
        {
            var errorPayload = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to authenticate user {Email}. Status: {StatusCode}. Response: {Response}",
                loginInfo.Username, response.StatusCode, errorPayload);

            if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized("Invalid username or password.");
            }

            return StatusCode(StatusCodes.Status502BadGateway, "Failed to authenticate with Firebase.");
        }

        GoogleToken? encoded;
        try
        {
            encoded = await response.Content.ReadFromJsonAsync<GoogleToken>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Firebase response when authenticating user {Email}.", loginInfo.Username);
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to parse Google response.");
        }

        if (encoded is null)
        {
            _logger.LogError("Received empty Firebase response when authenticating user {Email}.", loginInfo.Username);
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to parse Google response.");
        }

        var avatar = AvatarDefaults.Normalize(encoded.photoUrl);

        var token = new Token
        {
            token_type = "Bearer",
            access_token = encoded.idToken,
            id_token = encoded.idToken,
            expires_in = int.Parse(encoded.expiresIn),
            refresh_token = encoded.refreshToken,
            avatar = avatar
        };

        return Ok(token);
    }
}
