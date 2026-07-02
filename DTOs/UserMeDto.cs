using System.ComponentModel.DataAnnotations;

namespace Entry.Auth.DTOs
{
  public class UserMeDto
  {
    [Required]
    public string Id { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(32)]
    [RegularExpression("^[a-zA-Z0-9_]+$")]
    public string? Username { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public bool EmailConfirmed { get; set; }

    [MaxLength(256)]
    public string? Avatar { get; set; }

    [MaxLength(64)]
    public string? DisplayName { get; set; }

    public bool Premium { get; set; }
  }
}
