using System.ComponentModel.DataAnnotations;

namespace Entry.Auth.DTOs
{
  public class RegisterDto
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
  }
}