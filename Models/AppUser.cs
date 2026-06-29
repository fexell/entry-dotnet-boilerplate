using Microsoft.AspNetCore.Identity;

namespace Entry.Auth.Models
{
  public class AppUser : IdentityUser
  {
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}