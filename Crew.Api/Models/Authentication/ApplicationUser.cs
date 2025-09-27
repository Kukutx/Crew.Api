using Microsoft.AspNetCore.Identity;

namespace Crew.Api.Models.Authentication;

public class ApplicationUser : IdentityUser
{
    public string FirebaseUserId { get; set; }
    public string Name { get; set; }
}