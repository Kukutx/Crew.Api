using Crew.Api.Models;

namespace Crew.Api.Models.Authentication;

public class LoginInfo
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class FireBaseLoginInfo
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool ReturnSecureToken { get; set; } = true;
}

public class GoogleToken
{
    public string kind { get; set; } = string.Empty;
    public string localId { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string displayName { get; set; } = string.Empty;
    public string idToken { get; set; } = string.Empty;
    public bool registered { get; set; }
    public string refreshToken { get; set; } = string.Empty;
    public string expiresIn { get; set; } = string.Empty;
    public string? photoUrl { get; set; }
}

public class Token
{
    internal string refresh_token;

    public string token_type { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public int ext_expires_in { get; set; }
    public string access_token { get; set; } = string.Empty;
    public string id_token { get; set; } = string.Empty;
    public string avatar { get; set; } = AvatarDefaults.FallbackUrl;
}

public class LoginDetail
{
    public string FirebaseId { get; set; } = string.Empty;
    public string AspNetIdentityId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; }
}
