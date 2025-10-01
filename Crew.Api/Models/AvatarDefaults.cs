namespace Crew.Api.Models;

public static class AvatarDefaults
{
    public const string FallbackUrl = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    public static string Normalize(string? avatar)
        => string.IsNullOrWhiteSpace(avatar) ? FallbackUrl : avatar.Trim();
}
