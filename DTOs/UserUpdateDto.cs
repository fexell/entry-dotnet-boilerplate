using System.ComponentModel.DataAnnotations;

namespace Entry.Auth.DTOs
{
  public class UserUpdateDto
  {
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    [MaxLength(32, ErrorMessage = "Username must be at most 32 characters long")]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string? Username { get; set; }

    [MaxLength(64)]
    public string? DisplayName { get; set; }

    [MaxLength(256)]
    public string? Avatar { get; set; }
  }
}
