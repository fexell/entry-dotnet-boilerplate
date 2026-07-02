

namespace Entry.Auth.DTOs
{
  public class AuthResultDto
  {
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }

    public int ExpiresIn { get; set; }

    public UserMeDto? User { get; set; }
  }
}
