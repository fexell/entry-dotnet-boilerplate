using System.ComponentModel.DataAnnotations;

namespace Entry.Auth.DTOs
{
  public class RefreshDto
  {
    [Required(ErrorMessage = "Refresh token is required")]
    [MinLength(32, ErrorMessage = "Refresh token is too short")]
    [MaxLength(512, ErrorMessage = "Refresh token is too long")]
    [DataType(DataType.Text)]
    public string? RefreshToken { get; set; }
  }
}
