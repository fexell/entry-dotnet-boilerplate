

namespace Entry.Auth.DTOs
{
  public class VerifyEmailDto
  {
    public string UserId { get; set; } = default!;
    public string Token { get; set; } = default!;
  }
}