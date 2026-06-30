using System.ComponentModel.DataAnnotations;

namespace Entry.Auth.DTOs
{
  public class RefreshDto
  {
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
  }
}