using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Crew.Api.Models.RemoteConfig;

public class CreateRemoteConfigDisclaimerRequest
{
    [Required]
    [MaxLength(2048)]
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [MaxLength(20)]
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}
