using System.ComponentModel.DataAnnotations;

namespace Entry.Auth.DTOs
{
  public class VerifyEmailDto
  {
    [Required]
    public string UserId { get; set; } = default!;

    [Required]
    public string Token { get; set; } = default!;
  }
}