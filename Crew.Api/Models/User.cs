namespace Crew.Api.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Uid { get; set; } = ""; 
    public string Name { get; set; } = "";
    public string Bio { get; set; } = ""; 
    public string Avatar { get; set; } = ""; 
    public string Cover { get; set; } = ""; 
    public int Followers { get; set; } = 0; 
    public int Following { get; set; } = 0;
    public int Likes { get; set; } = 0; 
    public bool Followed { get; set; } = false; 
}
