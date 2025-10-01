using System.Text.Json;
using Crew.Api.Models;
using Crew.Api.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crew.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class AuthController : Controller
{
    [HttpPost]
    public async Task<ActionResult> GetToken([FromForm] LoginInfo loginInfo)
    {
        var fireBaseApiKey = "AIzaSyDp5ZOL1NsXlK3WMHmvS15GnnnnI4DGRFE";

        string uri = $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={fireBaseApiKey}";
        using (HttpClient client = new HttpClient())
        {
            FireBaseLoginInfo fireBaseLoginInfo = new FireBaseLoginInfo
            {
                Email = loginInfo.Username,
                Password = loginInfo.Password
            };
            var result = await client.PostAsJsonAsync(uri, fireBaseLoginInfo,
                new JsonSerializerOptions()
                    { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            var encoded = await result.Content.ReadFromJsonAsync<GoogleToken>();
            if (encoded is null)
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Failed to parse Google response.");
            }

            var avatar = AvatarDefaults.Normalize(encoded.photoUrl);

            Token token = new Token
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
}