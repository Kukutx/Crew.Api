namespace Crew.Api.Configuration;

public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; set; } = string.Empty;

    public string? ClientId { get; set; }

    public string? CredentialsPath { get; set; }

    public string? CredentialsJson { get; set; }
}
